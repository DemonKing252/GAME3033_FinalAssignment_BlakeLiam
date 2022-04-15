using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sfx
{
    MainTheme,
    BtnClick,
    Fire,
    Pickup,
    Reload,
    Hit
}



public class AudioManager : MonoBehaviour
{
    private static AudioManager sInstance;
    public static AudioManager Instance => sInstance;

    [SerializeField]
    private List<AudioSource> soundEffects;

    void Awake()
    {
        //sInstance = this;

        if (sInstance != null && sInstance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            sInstance = this;
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        PlaySound(Sfx.MainTheme, true);
    }

    public void PlaySound(Sfx sfx, bool loop = false, float playIn = 0f)
    {
        StartCoroutine(PlayInX(sfx, loop, playIn));
    }
    private IEnumerator PlayInX(Sfx sfx, bool loop, float playIn)
    {
        yield return new WaitForSeconds(playIn);

        soundEffects[(int)sfx].loop = loop;
        soundEffects[(int)sfx].Play();
    }
    public void StopSound(Sfx sfx)
    {
        soundEffects[(int)sfx].loop = false;
        soundEffects[(int)sfx].Stop();
    }

}
