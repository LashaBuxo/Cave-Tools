// Examples of VideoPlayer function

using UnityEngine;
using UnityEngine.Video;

public class Player : MonoBehaviour
{
    private VideoSynchroniser Synchroniser;
    public string ConfigFile = "config.cfg";
    public string videoFile = "video.mp4";
    public int TickDelay = 3;
    public int TicksTillConverge = 1;
    public float AccumulatedTime = 0;
    public string ServerIPAddress = "192.168.0.200";
    public int ServerPort = 10001;
    private VideoPlayer VPlayer;

    void Start()
    {
        // Will attach a VideoPlayer to the main camera.
        GameObject camera = GameObject.Find("Main Camera");

        // VideoPlayer automatically targets the camera backplane when it is added
        // to a camera object, no need to change videoPlayer.targetCamera.
        VPlayer = camera.AddComponent<UnityEngine.Video.VideoPlayer>();

        // Play on awake defaults to true. Set it to false to avoid the url set
        // below to auto-start playback since we're in Start().
        VPlayer.playOnAwake = false;

        // By default, VideoPlayers added to a camera will use the far plane.
        // Let's target the near plane instead.
        VPlayer.renderMode = UnityEngine.Video.VideoRenderMode.CameraNearPlane;

        // This will cause our Scene to be visible through the video being played.
        VPlayer.targetCameraAlpha = 0.5F;

        // Set the video to play. URL supports local absolute or relative paths.
        // Here, using absolute.
        VPlayer.url = videoFile;

        // Restart from beginning when done.
        VPlayer.isLooping = true;

        // Each time we reach the end, we slow down the playback by a factor of 10.
        VPlayer.loopPointReached += EndReached;

        // Start playback. This means the VideoPlayer may have to prepare (reserve
        // resources, pre-load a few frames, etc.). To better control the delays
        // associated with this preparation one can use videoPlayer.Prepare() along with
        // its prepareCompleted event.
        VPlayer.Play();

        VideoSynchroniser.InitializeSingleton(ConfigFile, TickDelay, TicksTillConverge, VPlayer, ServerIPAddress);
    }

    //public void SetFrame(int frame)
    //{
    //    Debug.Log(("Video frame adjusted from {0} to {1}", VPlayer.frame, frame));
    //    VPlayer.frame = frame;
    //}

    //public long GetFrame()
    //{
    //    Debug.Log(("Video frame is {0}", VPlayer.frame));
    //    return VPlayer.frame;
    //}

    void EndReached(UnityEngine.Video.VideoPlayer vp)
    {
        vp.playbackSpeed = vp.playbackSpeed / 10.0F;
    }

    private void Update()
    {
        AccumulatedTime += Time.deltaTime;
        if (AccumulatedTime > TickDelay)
        {
            AccumulatedTime = 0;
            int receivedFrame = VideoSynchroniser.Instance().IncreaseCurrentTick((int)VPlayer.frame);
            if (receivedFrame != -1)
            {
                VPlayer.frame = receivedFrame;
            }
        }
    }
}