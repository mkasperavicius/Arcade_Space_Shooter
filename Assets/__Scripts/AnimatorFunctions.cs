﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorFunctions : MonoBehaviour
{
    public MenuButtonController menuButtonController;
    
    // script just to play an Audio clip from an animation event. Takes a reference to a menuButtonController, and allows animation events to call this method.
    void PlaySound(AudioClip sound)
    {
        menuButtonController.audioSource.PlayOneShot(sound);
    }
}
