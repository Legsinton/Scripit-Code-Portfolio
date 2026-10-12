using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
//Alex
public class SoundFXManager : MonoBehaviour
{
    public static SoundFXManager Instance;

    [SerializeField]
    AudioSource soundFXObject;
    [SerializeField]
    AudioSource soundFXObjectButton;

    bool isQuitting = false;
    bool changeBackPitch;

    [SerializeField]
    List<SoundVolumeEntry> volumeList;
    [SerializeField] private AudioMixerGroup sfxMixerGroup;

    Dictionary<SoundType, object> soundFXDict;
    // Dictionary for volume on sounds
    Dictionary<SoundType, float> soundVolumeDict = new();
    // For Looping soundFX
    Dictionary<GameObject, LoopSoundInstance> activeLoopsByObject = new();

    public class LoopSoundInstance
    {   // Values for the looping soundfX's
        public AudioSource source;      // First AudioSource
        public AudioSource source2;     // Second AudioSource
        public GameObject obj;          // The GameObject holding the AudioSource
        public bool isTwoAudioClips;    // If its two audioclips playing at the same time
        public bool isChanging;         // If its changing volume or not
        public bool fadeToLoop1;        
        public float fadeTimer = 2;
        public float fadeDuration = 0;  //How long fade in/out shuld be
        public bool stop;               // Used to Stop Loop
        public bool changePitchBack;
        public bool pitchAmountBool;
        public float targetVolume;
        public float pitchChangeBackAmount;
    }

    [System.Serializable]
    public class SoundVolumeEntry
    {
        public SoundType soundType;  // The sound effect type (key)
        [Range(0, 1)] public float volume;         // The volume level (value)
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        foreach (var entry in volumeList)
        {
            soundVolumeDict[entry.soundType] = entry.volume;
        }

        InitializeSounds();
    }
    private void Update()
    {
        if (activeLoopsByObject.Count > 0 && activeLoopsByObject.Any(kv => kv.Value.isChanging || kv.Value.stop))
        {
            LoopFade();
        }

        if(activeLoopsByObject.Count > 0 && activeLoopsByObject.Any(kv => kv.Value.changePitchBack))
        {
            Debug.Log("Change pitch start");
            ChangeBackPitch();
        }
    }

    private void OnValidate()
    {   // To change volume while playing
        if (volumeList != null)
        {
            soundVolumeDict = new Dictionary<SoundType, float>();
            foreach (var entry in volumeList)
            {
                soundVolumeDict[entry.soundType] = entry.volume;
            }
        }
    }

    void ChangeBackPitch()
    {
        foreach (var entry in activeLoopsByObject)
        {
            LoopSoundInstance loop = entry.Value;

            if (loop.changePitchBack && loop.source.pitch > 1f && loop.pitchAmountBool)
            {
                Debug.Log("Changing pitch now ");

                loop.source.pitch -= loop.pitchChangeBackAmount * Time.deltaTime;
                if (loop.source2 != null)
                {
                    loop.source2.pitch -= loop.pitchChangeBackAmount * Time.deltaTime;
                }
            }
            else if(loop.pitchAmountBool)
            {
                Debug.Log("Stopping pitch change");
                loop.changePitchBack = false;
                loop.source.pitch = 1;
                if (loop.source2 != null)
                {
                    loop.source2.pitch = 1;
                }
            }
            if (loop.changePitchBack && loop.source.pitch < 1f && !loop.pitchAmountBool)
            {
                loop.source.pitch += loop.pitchChangeBackAmount * Time.deltaTime;
                if (loop.source2 != null)
                {
                    loop.source2.pitch += loop.pitchChangeBackAmount * Time.deltaTime;
                }
            }
            else if (!loop.pitchAmountBool)
            {
                Debug.Log("Stopping pitch change");
                loop.changePitchBack = false;
                loop.source.pitch = 1;
                if (loop.source2 != null)
                {
                    loop.source2.pitch = 1;
                }
            }
            if (changeBackPitch && soundFXObject.pitch > 1f)
            {
                soundFXObject.pitch -= loop.pitchChangeBackAmount * Time.deltaTime;
            }
            else
            {
                changeBackPitch = false;
            }
        }
    }

    private void InitializeSounds()
    {   // To find and add soundFX in assets
        soundFXDict = new Dictionary<SoundType, object>
        {
                //Single AudioClips
                { SoundType.Death, Resources.Load<AudioClip>("Sounds/Effects/Death") },
                { SoundType.Coin, Resources.Load<AudioClip>("Sounds/Effects/CoinPling") },
 
                //AudioClips
                { SoundType.Boing, Resources.LoadAll<AudioClip>("Sounds/Effects/Boing") },
                { SoundType.Smack, Resources.LoadAll<AudioClip>("Sounds/Effects/Smack") },
                { SoundType.Bonk, Resources.LoadAll<AudioClip>("Sounds/Effects/Bonk") },
                { SoundType.Splat, Resources.LoadAll<AudioClip>("Sounds/Effects/Splat") },
                { SoundType.Launch, Resources.LoadAll<AudioClip>("Sounds/Effects/Launch") }   
            
        };
    }
    private void LoopFade()
    {
        // Update fading for each active loop
        List<GameObject> objectsToRemove = new List<GameObject>(); // List to store objects to be removed

        foreach (var entry in activeLoopsByObject)
        {
            LoopSoundInstance loop = entry.Value;

            if(!loop.isTwoAudioClips && loop.isChanging)
            {
                loop.fadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(loop.fadeTimer / loop.fadeDuration);
                //float tO = Mathf.Clamp01(loop.fadeTimer / loop.fa);
                if (!loop.stop)
                {
                    loop.source.volume = Mathf.Lerp(0, loop.targetVolume, t);
                }
                else
                {
                    loop.source.volume = Mathf.Lerp(loop.targetVolume, 0, t);
                }

                if(t >= 1f)
                {
                    loop.isChanging = false;
                    loop.fadeTimer = 0;
                }
            }
            if(loop.isTwoAudioClips && loop.isChanging && !loop.stop)
            {
                loop.fadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(loop.fadeTimer / loop.fadeDuration);
                if (loop.fadeToLoop1)
                {
                    loop.source.volume = Mathf.Lerp(0, loop.targetVolume, t);
                    loop.source2.volume = Mathf.Lerp(loop.targetVolume, 0, t);
                }
                else
                {
                    loop.source.volume = Mathf.Lerp(loop.targetVolume, 0, t);
                    loop.source2.volume = Mathf.Lerp(0, loop.targetVolume, t);
                }

                if (t >= 1f)
                {
                    loop.isChanging = false;
                    loop.fadeTimer = 0;
                }
            }

            else if(loop.isTwoAudioClips && loop.stop)
            {
                loop.fadeTimer += Time.deltaTime;
                float t = Mathf.Clamp01(loop.fadeTimer / loop.fadeDuration);

                loop.source.volume = Mathf.Lerp(loop.targetVolume, 0, t);
                loop.source2.volume = Mathf.Lerp(loop.targetVolume, 0, t);
            }

            // If fade-out is complete, stop and mark the object for later removal
            if (loop.stop && loop.fadeTimer <= 0f)
            {
                loop.source.Stop();
                if (loop.source2 != null)
                {
                    loop.source2.Stop();
                }
                if (!isQuitting)
                {
                    Destroy(loop.obj); // Clean up the temporary GameObject
                }
                objectsToRemove.Add(entry.Key); // Mark for removal here
            }
            else if(loop.stop && loop.fadeTimer >= loop.fadeDuration)
            {
                loop.source.Stop();
                if (loop.source2 != null)
                {
                    loop.source2.Stop();
                }
                if (!isQuitting)
                {
                    Destroy(loop.obj); // Clean up the temporary GameObject
                }
                objectsToRemove.Add(entry.Key); // And Mark for removal here to
            }
        }
        // Remove from dictionary after iteration
        foreach (var obj in objectsToRemove)
        {
            activeLoopsByObject.Remove(obj);
        }
    }
                           //SoundType     // Position for soundFX    // soundFX minDistance  // soundFX MaxDistance   // If 2D or 3D   // If you want to change pitch
    public void PlaySoundFX(SoundType type, Vector3? position = null, float minSize = 1f, float maxSize = 50f, float spatialBlend = 1f, float pitch = 1)
    {
        if (!soundFXDict.ContainsKey(type)) return;

        if (isQuitting) return;

        // volume if its not in the inspector
        float volume = 1.0f;
        if (soundVolumeDict != null && soundVolumeDict.ContainsKey(type))
        {
            volume = soundVolumeDict[type];
        }

        AudioClip clip = null;
        if (soundFXDict[type] is AudioClip singleClip)
        {   // If single soundFX
            clip = singleClip;
        }
        else if (soundFXDict[type] is AudioClip[] clipArray)
        {   // If Multible soundFX
            clip = clipArray[Random.Range(0, clipArray.Length)];
        }

        if (clip == null) return;

        if (position.HasValue)
        {
            // Manual PlayClipAtPoint with custom values
            GameObject tempGO = new GameObject($"SFX_{type}");
            tempGO.transform.position = position.Value;

            AudioSource aSource = tempGO.AddComponent<AudioSource>();
            aSource.outputAudioMixerGroup = sfxMixerGroup;
            aSource.clip = clip;
            aSource.spatialBlend = spatialBlend;
            aSource.minDistance = minSize;
            aSource.maxDistance = maxSize;
            aSource.pitch = pitch;
            aSource.volume = volume;
            aSource.Play();

            Object.Destroy(tempGO, clip.length);
        }
        else
        {
            soundFXObject.PlayOneShot(clip, volume);
        }
    }

    public void ChangeSoundFXLoopVolume(GameObject owner, float fadeTime, bool death)
    {
        if (!activeLoopsByObject.ContainsKey(owner)) return;

        LoopSoundInstance instance = activeLoopsByObject[owner];


        if (death)
        {
            instance.source.volume -= fadeTime;
        }
        else
        {
            instance.source.volume += fadeTime;
        }
    }

    public void PlayButtonSoundFX(SoundType type)
    {
        if (!soundFXDict.ContainsKey(type)) return;

        // volume if its not in the inspector
        float volume = 1.0f;
        if (soundVolumeDict != null && soundVolumeDict.ContainsKey(type))
        {
            volume = soundVolumeDict[type];
        }

        AudioClip clip = null;
        if (soundFXDict[type] is AudioClip singleClip)
        {   // If single soundFX
            clip = singleClip;
        }
        else if (soundFXDict[type] is AudioClip[] clipArray)
        {   // If Multible soundFX
            clip = clipArray[Random.Range(0, clipArray.Length)];
        }

        if (clip == null) return;

        else
        {
            soundFXObjectButton.PlayOneShot(clip, volume);
        }
    }                                                                                                                                             // Size for Audio Source                                                                              
                            // GameObject Owner //SoundType  // Transform parent       // Fade Duration              // If 2D or 3D             // soundFX minSize  // soundFX MaxSize 
    public void StartLoopFor(GameObject owner, SoundType type, Transform parent = null, float setFadeDuration = 0.1f, float? spatialBlend = 1f, float? minSize = 1, float? maxSize = 50f)
    {
        if (activeLoopsByObject.ContainsKey(owner)) return; // If there's already a loop for this object, don't start another

        float volume = 1.0f;
        if (soundVolumeDict != null && soundVolumeDict.ContainsKey(type))
        {
            volume = soundVolumeDict[type];
        }
        AudioClip clip = null;
        if (!soundFXDict.ContainsKey(type))
        {
            Debug.LogError($"[SoundFXManager] soundFXDict does not contain key: {type}");
            return;
        }

        if (soundFXDict[type] is AudioClip singleClip)
        {
            clip = singleClip;
        }
        else if (soundFXDict[type] is AudioClip[] clipArray)
        {
            clip = clipArray[Random.Range(0, clipArray.Length)];
        }

        if (clip == null)
        {
            Debug.LogError($"[SoundFXManager] AudioClip for {type} is null.");
            return;
        }
        GameObject tempGO = new GameObject($"3D_Loop_{owner.name}_{type}");
        if (parent != null)
        {
            tempGO.transform.SetParent(parent);
            tempGO.transform.localPosition = Vector3.zero; // So it's centered on parent
        }
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.clip = clip;
        source.loop = true;
        if (spatialBlend.HasValue)
        {
            source.spatialBlend = spatialBlend.Value;
        }
        else
        {
            source.spatialBlend = 1f;
        }
        if (maxSize.HasValue)
        {
            source.maxDistance = maxSize.Value;
        }
        else
        {
            source.maxDistance = 50f;
        }
        if (minSize.HasValue)
        {
            source.minDistance = minSize.Value;
        }
        else
        {
            source.minDistance = 1;
        }
        //source.minDistance = 1f;
        //source.maxDistance = 50f;
        source.volume = 0;
        source.playOnAwake = false;
        source.Play();

        LoopSoundInstance instance = new LoopSoundInstance
        {
            source = source,
            isChanging = true,
            obj = tempGO,
            fadeTimer = 0f,
            fadeDuration = setFadeDuration,
            stop = false,
            targetVolume = volume
        };

        // Add the LoopSoundInstance to the dictionary
        activeLoopsByObject.Add(owner, instance);
    }
                            // The owner in which loop to stop
    public void StopLoopFor(GameObject owner, float fadeDuration = 0.15f)
    {
        if (!activeLoopsByObject.ContainsKey(owner)) return; // No loop found for this object

        LoopSoundInstance instance = activeLoopsByObject[owner];
        instance.stop = true; // Mark it for stopping (fade out)
        instance.isChanging = true;
        instance.fadeTimer = 0;
        instance.fadeDuration = fadeDuration;
    }                                                                                                                                                                                   // Size for Audio Source                                                                              
                                // GameObject Owner //SoundType  // Transform parent       // Fade Duration              // Which track to play first    // If 2D or 3D             // soundFX minSize  // soundFX MaxSize 
    public void StartTwoLoopsFor(GameObject owner, SoundType type, Transform parent = null, float setFadeDuration = 0.1f, bool startLoop1 = true, float? spatialBlend = 1f, float? minSize = 1, float? maxSize = 50f)
    {
        if (activeLoopsByObject.ContainsKey(owner)) return; // If there's already a loop for this object, don't start another

        float volume = 1.0f;
        if (soundVolumeDict != null && soundVolumeDict.ContainsKey(type))
        {
            volume = soundVolumeDict[type];
        }
        AudioClip[] clip = null;
        GameObject tempGO = new GameObject($"3D_Loop_{owner.name}_{type}");
         
        if (!soundFXDict.ContainsKey(type))
        {
            Debug.LogError($"[SoundFXManager] soundFXDict does not contain key: {type}");
            return;
        }

        if (soundFXDict[type] is AudioClip[] clipArray)
        {
            //clip = clipArray[Random.Range(0, clipArray.Length)];

            clip = clipArray;
            
        }

        if (clip == null || clip.Length < 2)
        {
            Debug.LogError($"[SoundFXManager] AudioClip for {type} is null.");
            return;
        }
        //GameObject tempGO = new GameObject($"3D_Loop_{owner.name}_{type}");
        if (parent != null)
        {
            tempGO.transform.SetParent(parent);
            tempGO.transform.localPosition = Vector3.zero; // So it's centered on parent
        }
        AudioSource source1 = tempGO.AddComponent<AudioSource>();
        AudioSource source2 = tempGO.AddComponent<AudioSource>();
        source1.outputAudioMixerGroup = sfxMixerGroup;
        source1.clip = clip[0];
        source1.loop = true;
        source2.outputAudioMixerGroup = sfxMixerGroup;
        source2.clip = clip[1];
        source2.loop = true;
        if (spatialBlend.HasValue)
        {
            source1.spatialBlend = spatialBlend.Value;
            source2.spatialBlend = spatialBlend.Value;
        }
        else
        {
            source1.spatialBlend = 1f;
            source2.spatialBlend = 1f;
        }
        if (maxSize.HasValue)
        {
            source1.maxDistance = maxSize.Value;
            source2.maxDistance = maxSize.Value;
        }
        else
        {
            source1.maxDistance = 50f;
            source2.maxDistance = 50f;
        }
        if (minSize.HasValue)
        {
            source1.minDistance = minSize.Value;
            source2.minDistance = minSize.Value;
        }
        else
        {
            source1.minDistance = 1;
            source2.minDistance = 1;
        }
        
        source1.volume = 0;
        source2.volume = 0;
        source1.Play();
        source2.Play();

        LoopSoundInstance instance = new LoopSoundInstance
        {
            isTwoAudioClips = true,
            isChanging = true,
            fadeToLoop1 = startLoop1,
            source = source1,
            source2 = source2,
            obj = tempGO,
            fadeTimer = 0f,
            fadeDuration = setFadeDuration,
            stop = false,
            targetVolume = volume
        };

        // Add the LoopSoundInstance to the dictionary
        activeLoopsByObject.Add(owner, instance);
    }                                                // true = first track, false = second track
                                     // Object Owner // bool for wich track to change // fadeTime is how long the fade in should be
    public void SwitchVolumeForLoops(GameObject owner, bool loop1, float? fadeTime = 0.1f)
    {
        if (!activeLoopsByObject.ContainsKey(owner)) return;

        LoopSoundInstance instance = activeLoopsByObject[owner];
        Debug.Log("Im switcing volumes");
        instance.fadeTimer = 0;
        instance.fadeDuration = fadeTime.Value;
        instance.fadeToLoop1 = loop1;
        instance.isChanging = true;
    }

    public void ChangePitchForSoundObject(GameObject owner, float pitch,bool? changeBackPitch = false, float pitchAmountBack = 0.5f)
    {
        if (!activeLoopsByObject.ContainsKey(owner)) return;

        LoopSoundInstance instance = activeLoopsByObject[owner];

        instance.source.pitch += pitch;
        instance.pitchChangeBackAmount = pitchAmountBack;

        if(instance.source2 != null)
        {
            instance.source2.pitch += pitch;
        }

        if (changeBackPitch != null)
        {
            instance.changePitchBack = changeBackPitch.Value;
            if(pitch > 0)
            {
                instance.pitchAmountBool = true;
            }
            else
            {
                instance.pitchAmountBool = false;
            }
        }
    }
    private void OnApplicationQuit()
    {
        isQuitting = true;
        StopLoopFor(gameObject);
    }
}

public enum SoundType
{
        // Array Of Sounds
        Break,
        Bonk,
        Boing,
        Coin,
        Death,
        Launch,
        Smash, 
        Smack,
        Splat
}
