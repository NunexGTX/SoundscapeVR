import os
import sys
import tkinter as tk
from tkinter import filedialog
import ffmpeg

OUTPUT_FOLDER_NAME = "Audio-Video-Split"

VIDEO_CODEC_TO_EXT = {
    "h264": ".mp4", "hevc": ".mp4", "av1": ".mp4", "vp8": ".webm",
    "vp9": ".webm", "mpeg4": ".mp4", "prores": ".mov",
}

AUDIO_CODEC_TO_EXT = {
    "aac": ".m4a", "mp3": ".mp3", "opus": ".opus", "vorbis": ".ogg",
    "flac": ".flac", "ac3": ".ac3", "alac": ".m4a",
}

def pick_file() -> str:
    root = tk.Tk()
    root.withdraw()
    root.attributes("-topmost", True)
    path = filedialog.askopenfilename(
        title="Select a media file",
        filetypes=[("Media files", "*.mp4 *.mkv *.mov *.avi *.webm *.ts"), ("All files", "*.*")],
    )
    root.destroy()
    if not path:
        sys.exit(0)
    return path

def get_stream_info(input_path: str):
    try:
        probe = ffmpeg.probe(input_path)
    except ffmpeg.Error as e:
        print(f"Error probing file: {e.stderr.decode()}")
        sys.exit(1)

    streams = probe.get("streams", [])
    video_stream = next((s for s in streams if s["codec_type"] == "video"), None)
    audio_streams = [s for s in streams if s["codec_type"] == "audio"]

    return video_stream, audio_streams

def process_file(input_path: str):
    video_info, audio_infos = get_stream_info(input_path)
    
    base_name = os.path.splitext(os.path.basename(input_path))[0]
    output_dir = os.path.join(os.path.dirname(os.path.abspath(input_path)), OUTPUT_FOLDER_NAME)
    os.makedirs(output_dir, exist_ok=True)

    # 1. Extract Video (if exists)
    if video_info:
        ext = VIDEO_CODEC_TO_EXT.get(video_info["codec_name"], ".mkv")
        v_out = os.path.join(output_dir, f"{base_name}_video{ext}")
        print(f"▶ Extracting Video: {video_info['codec_name']} -> {ext}")
        ffmpeg.input(input_path).output(v_out, vcodec="copy", an=None).overwrite_output().run(quiet=True)

    # 2. Extract Each Audio Track
    for i, stream in enumerate(audio_infos):
        codec = stream.get("codec_name", "unknown")
        channels = stream.get("channels", 2)
        layout = stream.get("channel_layout", "")
        
        # Ambisonic/Spatial Logic
        # YouTube Ambisonics often use 'ambisonic' layout or 4 channels in Opus
        is_ambisonic = "ambisonic" in layout or "ambisonic" in stream.get("tags", {}).get("handler_name", "").lower()
        
        # If it's more than 2 channels or explicitly ambisonic, force WAV conversion
        if is_ambisonic or channels > 2:
            ext = ".wav"
            acodec = "pcm_s24le" # High quality PCM to preserve spatial data
            mode = "Converting (Ambisonic/Multi-channel detected)"
        else:
            ext = AUDIO_CODEC_TO_EXT.get(codec, ".mka")
            acodec = "copy"
            mode = "Copying"

        a_out = os.path.join(output_dir, f"{base_name}_track_{i}_{codec}{ext}")
        print(f"▶ Processing Audio Track #{i}: {codec} ({channels}ch) -> {mode}")

        # Map only the specific audio stream using 'map'
        (
            ffmpeg
            .input(input_path)
            .output(a_out, map=f"0:a:{i}", acodec=acodec, vn=None)
            .overwrite_output()
            .run()
        )

    print("\nVideo and File have been Seperated. They did not live a long life.")

if __name__ == "__main__":
    file_path = pick_file()
    process_file(file_path)