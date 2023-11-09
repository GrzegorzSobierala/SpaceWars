using Game.Input.System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;
using Zenject;

namespace Game.Player.Ship
{
    public class A10MachineGunSound : MonoBehaviour
    {
        [Inject] InputProvider _input;

        [SerializeField] private AudioSource _startAudio;
        [SerializeField] private AudioSource _loopAudio;
        [SerializeField] private AudioSource _endAudio;
        [SerializeField] private AudioSource _afterAudio;

        protected PlayerControls.GameplayActions Input => _input.PlayerControls.Gameplay;

        private bool _isPlaying = false;
        private bool _isEnding = false;

        private void Update()
        {
            if(Input.Shoot.ReadValue<float>() == 1)
            {
                PlayingSound();
            }
            else
            {
                EndingSound();
            }
        }

        private void PlayingSound()
        {
            if (_startAudio.isPlaying)
                return;

            if (!_isPlaying)
            {
                _endAudio.Stop();
                _afterAudio.Stop();
                _startAudio.Play();
                _isPlaying = true;
                return;
            }

            if (_loopAudio.isPlaying)
                return;

            _loopAudio.Play();
        }

        private void EndingSound()
        {
            if (_endAudio.isPlaying)
            {
                Debug.Log("_endAudio.isPlaying");
                return;
            }

            if (_isPlaying)
            {
                _startAudio.Stop();
                _loopAudio.Stop();
                _endAudio.Play();
                _isPlaying = false;
                _isEnding = true;
                return;
            }

            if (!_isEnding)
                return;

            if (_afterAudio.isPlaying)
                return;

            _afterAudio.Play();
            _isEnding = false;
        }
    }
}
