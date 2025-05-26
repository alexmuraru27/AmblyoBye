Created with Unity 6000.0.35f1 
In order to convert the movies -> use HandBrake-1.9.0-x86_64-Win_GUI.exe 
-> convert to MP4 H.254 30 fps AAC audio (Unity supports different video formats, but this worked the best for me) 
-> supported [ ".asf", ".avi", ".dv", ".m4v", ".mp4", ".mov", ".mpg", ".mpeg", ".m4v", ".ogv", ".vp8", ".webm", ".wmv"](only some encodings, for me H254 worked well) 
-> I used Fast 1080p 30 preset, but you can also use quality ones

Open the application once, so it can create the directory structure 
Then add the mp4 movies in Quest 3\Internal shared storage\Android\data\com.bober101.amblyobye\Movies 
Relaunch the application and in the DichopticMovie category you should see your movie in the dropdown list

Screenshot from application running movies:

|            Left Eye            |            Right Eye             |
| :----------------------------: | :------------------------------: |
| ![left_eye](Docu/left_eye.jpg) | ![right_eye](Docu/right_eye.jpg) |

Settings Screen
![settings](Docu/settings.jpg)
