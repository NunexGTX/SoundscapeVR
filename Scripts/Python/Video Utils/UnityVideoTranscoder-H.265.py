import ffmpeg
import tkinter as tk
from tkinter import filedialog
import logging
from pathlib import Path
import json
import shutil

logging.basicConfig(level=logging.INFO)

transcoding_folder_name = "Unity Ready Videos"
vp8_unity_editor_folder_name = "Unity Editor Videos (VP8 .webm)"
build_videos = "Unity Player Videos (H.265 .mp4)"

max_videos_resolution = {
    "default": [-2,4320], #This tries to go with somewhat 8k but mainting the original aspect ratio if its not a registered one
    "16:9": [7680,4320],
    "2:1": [8192,4096]
}

#Leave videos at 8K max
transcoding_settings = {
    #container
    "video_container": ".mp4",

    #video
    "video_codec": "libx265", #Software encoding gets better results and works in any machine. Although slower
    "crf": 22, #Target rate coefficient (lower is better but higher filesize)
    "preset": "slow",

    #Video color
    "color_format":"yuv420p", #Keep colors in SDR (8bit), HDR (10bit) will probably not play nicely in unity
    
    #Frame rate
    "frame-rate-variable": 'cfr', #crf keeps the frame rate constant
    
    #audio
    "audio_codec": "libopus",
    "audio_bitrate": "256k",
    
    #Metadata
    "map_metadata": 0
}

max_vp8_resolution = {
    "default": [-2,1080], #This tries to go with somewhat 8k but mainting the original aspect ratio if its not a registered one
    "16:9": [1920,1080],
    "2:1": [1920,960]
}

unity_debug_vp8_transcoding_settings = {
    #container
    "video_container": ".webm",

    #video
    "video_codec": "libvpx", #Software encoding gets better results and works in any machine. Although slower #vp8 encoder
    "crf": 22, #Target rate coefficient (lower is better but higher filesize)

    #Video color
    "color_format":"yuv420p", #Keep colors in SDR (8bit), HDR (10bit) will probably not play nicely in unity
    
    #Frame rate
    "frame-rate-variable": 'cfr', #crf keeps the frame rate constant
    
    #audio
    "audio_codec": "libopus",
    "audio_bitrate": "256k",
    
    #Metadata
    "map_metadata": 0
}

def checkFFMPEG():
    ffmpeg_path = shutil.which('ffmpeg')
    ffprobe_path = shutil.which('ffprobe')

    if not ffmpeg_path or not ffprobe_path:
        print("Please install ffmpeg to use this script: https://ffmpeg.org/download.html")
        exit(1)

def init():
    print("Unity Video Transcoding")
    print("This script allows you to quickly transcode videos into a .mp4 with the H.265 codec to have the most efficient quality to watch in most devices. Up to 8K")
    print("It also makes a copy to .webm using the vp8 codec only at 1080p resolution so it can play smoothly and natively in Unity Editor for debug.")
    print()
    print()
    warning_messages()
    print()
    print()

def warning_messages():
    print("*****WARNINGS*****")
    print("Windows Warning: For Windows 10+ it is mandatory you have the HVENC Video Extension from Micosoft Store in order for Unity Player to be able to decode the H265 video.")
    print("Linux Warning: Unfortunatly H.265 is not supported on Linux. Either use AV1 or VP9 codecs. This script is currently not suitable for that platform.")

def video_picker() -> str:
    root = tk.Tk()
    root.withdraw()

    video_extensions = "*.mp4 *.mov *.avi *.mkv *.wmv *.flv *.mpeg *.mpg *.webm" #common video extensions

    file_path = filedialog.askopenfilename(
        title="Pick your video file",
        filetypes=[
            ("Video files", video_extensions),
            ("All files", "*.*")
        ]
    )

    root.destroy()

    if file_path:
        return file_path
    else:
        logging.error("No valid file was selected")
        return None

def make_output_folders(file_path: str):
    path = Path(file_path)
    folder_path = path.parent

    target_transcoding_folder = folder_path / transcoding_folder_name
    target_transcoding_folder.mkdir(parents=True,exist_ok=True) #Creates transcoding folder if it doesn't exist

    build_videos_folder = target_transcoding_folder / build_videos
    build_videos_folder.mkdir(parents=True,exist_ok=True)

    unity_editor_folder = target_transcoding_folder / vp8_unity_editor_folder_name
    unity_editor_folder.mkdir(parents=True,exist_ok=True)

    return str(build_videos_folder), str(unity_editor_folder)

def probeVideoData(video_file: str) -> str:
    print(json.dumps(ffmpeg.probe(video_file), indent=4))
    return ffmpeg.probe(video_file)

def get_aspect_ratio(probe_data):
    """Calculates aspect ratio to choose the right resolution bucket."""
    video_stream = next((s for s in probe_data['streams'] if s['codec_type'] == 'video'), None)
    if not video_stream:
        return "default"
    
    width = int(video_stream['width'])
    height = int(video_stream['height'])
    ratio = width / height

    if abs(ratio - (16/9)) < 0.1: return "16:9"
    if abs(ratio - (2/1)) < 0.1: return "2:1"
    return "default"

def transcode(video_file: str, unity_editor_video_path: str, build_videos_path: str, probe_video: str):
    video_name = Path(video_file).stem
    video_ratio = get_aspect_ratio(probe_video)

    unityEditorVideoPath = Path(unity_editor_video_path)
    buildVideosPath = Path(build_videos_path)

    target_output = buildVideosPath / f"{video_name}{transcoding_settings['video_container']}"
    editor_output = unityEditorVideoPath / f"{video_name}{unity_debug_vp8_transcoding_settings['video_container']}"

    res_target = max_videos_resolution.get(video_ratio, max_videos_resolution["default"])

    #Target transcoding
    ffmpeg.input(video_file).output(
        str(target_output),
        vcodec=transcoding_settings["video_codec"],
        crf=transcoding_settings["crf"],
        preset=transcoding_settings["preset"],
        pix_fmt=transcoding_settings["color_format"],
        vsync=transcoding_settings["frame-rate-variable"],
        acodec=transcoding_settings["audio_codec"],
        audio_bitrate=transcoding_settings["audio_bitrate"],
        map_metadata=transcoding_settings["map_metadata"],
        vf=f"scale='min(iw,{res_target[0]})':'min(ih,{res_target[1]})':force_original_aspect_ratio=decrease"
    ).overwrite_output().run()

    res_vp8 = max_vp8_resolution.get(video_ratio, max_vp8_resolution["default"])

    #vp8 unity editor transcoding
    ffmpeg.input(video_file).output(
        str(editor_output),
        vcodec=unity_debug_vp8_transcoding_settings["video_codec"],
        crf=unity_debug_vp8_transcoding_settings["crf"],
        vsync=unity_debug_vp8_transcoding_settings["frame-rate-variable"],
        acodec=unity_debug_vp8_transcoding_settings["audio_codec"],
        audio_bitrate=unity_debug_vp8_transcoding_settings["audio_bitrate"],
        pix_fmt=unity_debug_vp8_transcoding_settings["color_format"],
        map_metadata=unity_debug_vp8_transcoding_settings["map_metadata"],
        vf=f"scale='min(iw,{res_vp8[0]})':'min(ih,{res_vp8[1]})':force_original_aspect_ratio=decrease"
    ).overwrite_output().run()

def ask_confirm(question):
    while True:
        reply = input(f"{question} [Y/N]: ").lower().strip()
        
        if reply in ['y', 'yes']:
            return True
        if reply in ['n', 'no']:
            return False
        
        print("Invalid input. Please enter 'y' or 'n'.")

if __name__ == '__main__':
    checkFFMPEG()
    init()
    while True:
        while True:
            video_file = video_picker()
            if video_file:
                break
            else:
                exit(0)
        probe_video = probeVideoData(video_file)
        if ask_confirm("Convert this video?"):
            build_videos_path, unity_editor_video_path = make_output_folders(video_file)
            transcode(video_file,unity_editor_video_path,build_videos_path,probe_video)
        else:
            print("Skipping...")
        
        if not ask_confirm("Convert more videos?"):
            exit(0)
        
        