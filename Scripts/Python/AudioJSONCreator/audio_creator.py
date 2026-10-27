import json
import tkinter as tk
from tkinter import ttk, filedialog, messagebox

class SoundConfigApp:
    def __init__(self, root):
        self.root = root
        self.root.title("Sound JSON Config Generator")
        self.root.geometry("750x650")
        
        self.sounds_list = []
        
        # --- UI LAYOUT ---
        # Top Global Actions Frame
        global_frame = ttk.Frame(root, padding=10)
        global_frame.pack(fill="x")
        ttk.Button(global_frame, text="Import Existing JSON File...", command=self.import_json).pack(side="left", padx=5)
        
        # Form Frame
        form_frame = ttk.LabelFrame(root, text=" Add / Edit Sound ", padding=10)
        form_frame.pack(fill="x", padx=10, pady=5)
        
        # Form Inputs
        ttk.Label(form_frame, text="ID:").grid(row=0, column=0, sticky="w", pady=2)
        self.ent_id = ttk.Entry(form_frame, width=30)
        self.ent_id.grid(row=0, column=1, sticky="w", pady=2)
        
        ttk.Label(form_frame, text="Name:").grid(row=1, column=0, sticky="w", pady=2)
        self.ent_name = ttk.Entry(form_frame, width=30)
        self.ent_name.grid(row=1, column=1, sticky="w", pady=2)
        
        ttk.Label(form_frame, text="Type:").grid(row=2, column=0, sticky="w", pady=2)
        self.type_var = tk.StringVar(value="standard")
        self.cmb_type = ttk.Combobox(form_frame, textvariable=self.type_var, values=["standard", "ambisonic"], state="readonly", width=28)
        self.cmb_type.grid(row=2, column=1, sticky="w", pady=2)
        self.cmb_type.bind("<<ComboboxSelected>>", self.toggle_type_fields)
        
        ttk.Label(form_frame, text="File / Bundle Path:").grid(row=3, column=0, sticky="w", pady=2)
        self.ent_file = ttk.Entry(form_frame, width=50)
        self.ent_file.grid(row=3, column=1, sticky="w", pady=2)
        
        ttk.Label(form_frame, text="File Extension:").grid(row=4, column=0, sticky="w", pady=2)
        self.ent_ext = ttk.Entry(form_frame, width=10)
        self.ent_ext.insert(0, "wav")
        self.ent_ext.grid(row=4, column=1, sticky="w", pady=2)
        
        ttk.Label(form_frame, text="Image Path (Optional):").grid(row=5, column=0, sticky="w", pady=2)
        self.ent_image = ttk.Entry(form_frame, width=50)
        self.ent_image.grid(row=5, column=1, columnspan=2, sticky="w", pady=2)
        
        # Ambisonic-specific fields (hidden initially)
        self.lbl_amb_asset = ttk.Label(form_frame, text="Ambisonic Asset Name:")
        self.ent_amb_asset = ttk.Entry(form_frame, width=50)
        
        # Add Button
        self.btn_add = ttk.Button(form_frame, text="Add Entry to List", command=self.add_sound)
        self.btn_add.grid(row=7, column=1, sticky="e", pady=10)
        
        # Listview Frame
        list_frame = ttk.LabelFrame(root, text=" Current Sound Configuration Entries ", padding=10)
        list_frame.pack(fill="both", expand=True, padx=10, pady=5)
        
        self.tree = ttk.Treeview(list_frame, columns=("id", "name", "type", "file"), show="headings")
        self.tree.heading("id", text="ID")
        self.tree.heading("name", text="Name")
        self.tree.heading("type", text="Type")
        self.tree.heading("file", text="File/Bundle")
        self.tree.column("id", width=100)
        self.tree.column("name", width=150)
        self.tree.column("type", width=100)
        self.tree.column("file", width=250)
        self.tree.pack(fill="both", expand=True, side="left")
        
        scrollbar = ttk.Scrollbar(list_frame, orient="vertical", command=self.tree.yview)
        self.tree.configure(yscrollcommand=scrollbar.set)
        scrollbar.pack(fill="y", side="right")
        
        # Action Buttons
        btn_frame = ttk.Frame(root, padding=10)
        btn_frame.pack(fill="x")
        
        ttk.Button(btn_frame, text="Delete Selected", command=self.delete_sound).pack(side="left", padx=5)
        ttk.Button(btn_frame, text="Export JSON File...", command=self.export_json).pack(side="right", padx=5)

    def toggle_type_fields(self, event=None):
        if self.type_var.get() == "ambisonic":
            self.lbl_amb_asset.grid(row=6, column=0, sticky="w", pady=2)
            self.ent_amb_asset.grid(row=6, column=1, sticky="w", pady=2)
        else:
            self.lbl_amb_asset.grid_forget()
            self.ent_amb_asset.grid_forget()

    def add_sound(self):
        s_id = self.ent_id.get().strip()
        name = self.ent_name.get().strip()
        s_type = self.type_var.get()
        file_name = self.ent_file.get().strip()
        file_type = self.ent_ext.get().strip()
        image_path = self.ent_image.get().strip()
        
        if not s_id or not name or not file_name:
            messagebox.showerror("Error", "ID, Name, and File/Bundle Path are mandatory fields.")
            return

        entry = {
            "id": s_id,
            "name": name,
            "fileName": file_name,
            "fileType": "wav" if s_type == "ambisonic" else file_type,
            "type": s_type,
            "imagePath": image_path
        }
        
        if s_type == "ambisonic":
            amb_asset = self.ent_amb_asset.get().strip()
            if not amb_asset:
                messagebox.showerror("Error", "Ambisonic Asset Name is mandatory for ambisonic types.")
                return
            entry["ambisonicAssetName"] = amb_asset

        self.sounds_list.append(entry)
        self.tree.insert("", "end", values=(s_id, name, s_type, file_name))
        
        # Clear fields
        self.ent_id.delete(0, tk.END)
        self.ent_name.delete(0, tk.END)
        self.ent_file.delete(0, tk.END)
        self.ent_image.delete(0, tk.END)
        self.ent_amb_asset.delete(0, tk.END)

    def delete_sound(self):
        selected_item = self.tree.selection()
        if not selected_item:
            return
        for item in selected_item:
            index = self.tree.index(item)
            del self.sounds_list[index]
            self.tree.delete(item)

    def import_json(self):
        file_path = filedialog.askopenfilename(filetypes=[("JSON Files", "*.json")])
        if not file_path:
            return
            
        try:
            with open(file_path, "r", encoding="utf-8") as f:
                data = json.load(f)
                
            if "sounds" not in data or not isinstance(data["sounds"], list):
                raise ValueError("Missing 'sounds' array root in JSON architecture.")
                
            # Clear current UI layout entries to prepare for incoming file data
            self.sounds_list.clear()
            for item in self.tree.get_children():
                self.tree.delete(item)
                
            # Process parsed entries
            for sound in data["sounds"]:
                s_id = sound.get("id", "")
                name = sound.get("name", "")
                s_type = sound.get("type", "standard")
                file_name = sound.get("fileName", "")
                file_type = sound.get("fileType", "wav")
                image_path = sound.get("imagePath", "")
                
                entry = {
                    "id": s_id,
                    "name": name,
                    "fileName": file_name,
                    "fileType": "wav" if s_type == "ambisonic" else file_type,
                    "type": s_type,
                    "imagePath": image_path
                }
                
                if s_type == "ambisonic":
                    entry["ambisonicAssetName"] = sound.get("ambisonicAssetName", "")
                    
                self.sounds_list.append(entry)
                self.tree.insert("", "end", values=(s_id, name, s_type, file_name))
                
            messagebox.showinfo("Success", f"Successfully imported {len(data['sounds'])} sounds from file.")
            
        except Exception as e:
            messagebox.showerror("Import Error", f"Failed to process JSON schema:\n{str(e)}")

    def export_json(self):
        if not self.sounds_list:
            messagebox.showwarning("Warning", "No entries to export.")
            return
            
        file_path = filedialog.asksaveasfilename(defaultextension=".json", filetypes=[("JSON Files", "*.json")])
        if not file_path:
            return
            
        output = {"sounds": self.sounds_list}
        with open(file_path, "w", encoding="utf-8") as f:
            json.dump(output, f, indent=4)
        messagebox.showinfo("Success", f"Configuration saved successfully to:\n{file_path}")

if __name__ == "__main__":
    root = tk.Tk()
    app = SoundConfigApp(root)
    root.mainloop()