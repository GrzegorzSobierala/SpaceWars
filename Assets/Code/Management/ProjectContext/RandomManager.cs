using System;
using UnityEngine;

namespace Game.Management
{
    public class RandomManager : MonoBehaviour
    {
        private int _seed;

        public int Seed => _seed;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            _seed = (int)DateTime.Now.Ticks;
            UnityEngine.Random.InitState(_seed);
        }
    }
}
