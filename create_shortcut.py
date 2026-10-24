import os
import win32com.client

desktop = r"C:\Users\ali\Desktop"
bat_path = os.path.join(desktop, "تشغيل_نظام_نماذج_البصرة.bat")
work_dir = r"C:\Users\ali\.gemini\antigravity\scratch\basra_forms_system"
lnk_path = os.path.join(desktop, "منظومة نماذج البصرة 2026.lnk")

shell = win32com.client.Dispatch("WScript.Shell")
shortcut = shell.CreateShortCut(lnk_path)
shortcut.Targetpath = bat_path
shortcut.WorkingDirectory = work_dir
shortcut.WindowStyle = 1
shortcut.Description = "تشغيل منظومة نماذج مكتب انتخابات البصرة والمراكز الـ 76"
shortcut.IconLocation = "imageres.dll,114"
shortcut.save()

print("Created shortcut at:", lnk_path)
