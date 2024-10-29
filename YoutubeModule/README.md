# Youtube module

## Example of usage

```plang
Start
- monitor %status% for change, call StatusUpdate
- upload file.mp4, "My first video", "I am doing my first video.....", category 10
		put on "default" playlist, make it public and update %status% on the progress
- get all videos, write to %videos%
- get all videos in my "default" playlist, write to %videos%

StatusUpdate
- write out %status%
```


## Categories

Category id is required, here is list of available category ids

1 - Film & Animation  
2 - Autos & Vehicles  
10 - Music  
15 - Pets & Animals  
17 - Sports  
19 - Travel & Events  
20 - Gaming  
22 - People & Blogs  
23 - Comedy  
24 - Entertainment  
25 - News & Politics  
26 - Howto & Style  
27 - Education  
28 - Science & Technology  
29 - Nonprofits & Activism


## Playlist id

You can find the playlist id by going into your youtube studio -> Content -> Playlist

click the playlist you want to upload to, the URL in your browser will look something like this

https://studio.youtube.com/playlist/PLbm1UMZKMaqdOIjjdmz94x5BrGj2z9jQR/

the text starting and ending with "PLm1.....9jQR" is the playlist id. Dont include the /

