using DG.Tweening;
using Game.Combat;
using Game.Management;
using Game.Utility;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Drawing;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
using Zenject;
using static UnityEngine.Rendering.DebugUI;

namespace Game.Room.Enemy
{
    public class BasicEnemyGun : EnemyGunBase
    {
        public event Action OnStartReload;
        public event Action OnStopReload;

        [Inject] private PlayerManager _playerManager;
        [Inject] private RandomManager _randomManager;

        [SerializeField, AutoFill] private Transform _gunShootPoint;
        [SerializeField] private ShootableObjectBase _bulletPrototype;
        [Space, Header("Aim")]
        [SerializeField] private LayerMask _blockAimLayerMask;
        [SerializeField] private float _gunTraverse = 45f;
        [SerializeField] private float _rotateSpeed = 100f;
        [SerializeField] private float _aimedAngle = 4;
        [SerializeField] private float _aimRange = 500f;
        [SerializeField] private float _aimFollowTime = 2f;
        [SerializeField] LostTargetMode _lostTargetMode;
        [Space, Header("Shoot")]
        [SerializeField] private float _shotInterval = 0.5f;
        [SerializeField] private int _magCapacity = 5;
        [SerializeField] private float _reloadTime = 7f;
        [SerializeField] private float _shootAtMaxDistanceMutli = 0.7f;
        [SerializeField] private float _beforeReloadEventTime = 0.5f;
        [Space]
        [SerializeField] private UnityEvent _onBeforeReloaded;
        [SerializeField] private UnityEvent _onShootGun;

        private Coroutine _reloadCoroutine;
        private float _lastShotTime = 0f;
        private float _endReloadTime = 0f;
        private int _currenaMagAmmo = 0;
        private bool _wasOnBeforeReloadedCalled = false;
        private ContactFilter2D _contactFilter;
        private Vector3 _startForwardDir;

        protected override void Awake()
        {
            base.Awake();

            _currenaMagAmmo = _magCapacity;
            Initalize();
        }

        public override void Prepare()
        {
            _lastShotTime = Time.time;
        }

        protected override void OnAimingAt(Transform target)
        {
            base.OnAimingAt(target);

            if (IsKnowWherePlayerIs(target.position, _aimRange, _aimFollowTime, _contactFilter))
            {
                Aim(target.position, _gunTraverse, _rotateSpeed, _aimedAngle, true);
            }
            else
            {
                LostTargetAction(_lostTargetMode);
            }
        }

        public bool _oscilateTest;

        protected override void OnStopAiming()
        {
            base.OnStopAiming();
        }

        protected override void OnShooting()
        {
            base.OnShooting();

            TryShoot();
        }

        protected override void OnStopShooting()
        {
            base.OnStopShooting();

            StartReloading();
        }

        private void Initalize()
        {
            _contactFilter = new ContactFilter2D
            {
                useTriggers = false,
                layerMask = _blockAimLayerMask,
                useLayerMask = true,
            };

            _startForwardDir = transform.forward;
        }

        [Button]
        private void Shoot()
        {
            _lastShotTime = Time.time;

            GameObject damageDealer = _body.gameObject;
            Transform parent = _playerManager.transform;

            _onShootGun?.Invoke();

            _bulletPrototype.CreateCopy(damageDealer, parent).Shoot(_body, _gunShootPoint);

            _currenaMagAmmo--;

            OnShoot?.Invoke();

            if (_currenaMagAmmo == 0)
            {
                StartReloading();
            }
        }

        private void TryShoot()
        {
            if (Time.time - _lastShotTime < _shotInterval || _currenaMagAmmo <= 0)
                return;

            if (!IsAimedAtPlayer)
                return;

            float targetDistance = Vector2.Distance(_playerManager.PlayerBody.position, 
                transform.position);
            if (targetDistance > _bulletPrototype.MaxDistance * _shootAtMaxDistanceMutli)
                return;

            Shoot();
        }

        private bool TryReload()
        {
            if (!_wasOnBeforeReloadedCalled && Time.time > _endReloadTime - _beforeReloadEventTime)
            {
                _onBeforeReloaded?.Invoke();
                _wasOnBeforeReloadedCalled = true;
            }

            if (Time.time < _endReloadTime)
                return false;

            Reload();
            return true;
        }

        private void Reload()
        {
            _currenaMagAmmo = _magCapacity;
            OnStopReload?.Invoke();
            _wasOnBeforeReloadedCalled = true;
        }

        private void StartReloading()
        {
            if (_reloadCoroutine != null)
            {
                Debug.Log($"Reloading is already in progress. Time left {Time.time - _endReloadTime}");
                return;
            }

            _endReloadTime = Time.time + _reloadTime;
            OnStartReload?.Invoke();
            StartCoroutine(ReloadingMag());
        }

        private IEnumerator ReloadingMag()
        {
            yield return new WaitUntil(TryReload);

            _reloadCoroutine = null;
        }

        protected void LostTargetAction(LostTargetMode lostTargetMode)
        {
            switch (lostTargetMode)
            {
                case LostTargetMode.Stay:
                    LostTargetStay();
                    break;
                case LostTargetMode.Forward:
                    LostTargetActionForward();
                    break;
                case LostTargetMode.Search:
                    LostTargetActionSearch();
                    break;
                default:
                    Debug.LogError("Switch error");
                    break;
            }
        }

        private void LostTargetStay()
        {
            //Vector2 lookForwardPoint = _rotationTrans.transform.up + _rotationTrans.transform.position;
            //Aim(lookForwardPoint, _gunTraverse, _rotateSpeed, _aimedAngle, false);
        }

        private void LostTargetActionForward()
        {
            Aim(0, 360, _rotateSpeed, _aimedAngle, false);
        }

        public bool _goLeftSearch = true;

        private void LostTargetActionSearch()
        {


            if (Time.frameCount == 1)
                return;


            float rotSpeed = _rotateSpeed * 0.25f;
            float angleFullCycle = _gunTraverse * 2;
            float halfTravers = _gunTraverse / 2;
            float fullCycleTime = Utils.CalculateRotationTime(rotSpeed, angleFullCycle);
            float halfCycleTime = fullCycleTime / 2;

            int seed = GetInstanceID() + _randomManager.Seed;
            System.Random random = new System.Random(seed);
            float randomFloat = (float)random.NextDouble(); //0 - 1
            float timeCurSeededGlobal = Time.time /*+ (randomFloat * fullCycleTime)*/;
            float timePrevSeededGlobal = (timeCurSeededGlobal - Time.deltaTime);
            float timeCurSeededGlobalCycle = timeCurSeededGlobal % fullCycleTime;
            float timePrevSeededGlobalCycle = timePrevSeededGlobal % fullCycleTime;

            


            //Aim(testAngle, _gunTraverse, rotSpeed
            //    , _aimedAngle, false);


            this.Oscillate(-halfTravers, halfTravers, halfCycleTime, CurrentGunRot, AimOnOscillate);

            //Debug.Log($" |+| {possibleTimeMove1Fixed.ToString("f3")} | {possibleTimeMove2Fixed.ToString("f3")}| |+| {(possibleTimeMove1Fixed3).ToString("f3")} | {(possibleTimeMove2Fixed3).ToString("f3")}| {GetFixedTime(timePrevSeededGlobal).ToString("f3")}");

        }

        private void AimOnOscillate(float targetAngle)
        {
            float rotSpeed = _rotateSpeed * 0.25f;
            Aim(targetAngle, _gunTraverse, rotSpeed
                , _aimedAngle, false);

            Debug.Log($" |+| {CurrentGunRot.ToString("f3")} | {targetAngle.ToString("f3")}|");
        }

        //private void LostTargetActionSearch()
        //{


        //    if (Time.frameCount == 1)
        //        return;


        //    float rotSpeed = _rotateSpeed * 0.25f;
        //    float angleFullCycle = _gunTraverse * 2;
        //    float halfTravers = _gunTraverse / 2;
        //    float fullCycleTime = Utils.CalculateRotationTime(rotSpeed, angleFullCycle);
        //    float halfCycleTime = fullCycleTime / 2;

        //    int seed = GetInstanceID() + _randomManager.Seed;
        //    System.Random random = new System.Random(seed);
        //    float randomFloat = (float)random.NextDouble(); //0 - 1
        //    float timeCurSeededGlobal = Time.time /*+ (randomFloat * fullCycleTime)*/;
        //    float timePrevSeededGlobal = (timeCurSeededGlobal - Time.deltaTime);
        //    float timeCurSeededGlobalCycle = timeCurSeededGlobal % fullCycleTime;
        //    float timePrevSeededGlobalCycle = timePrevSeededGlobal % fullCycleTime;

        //    float anglePervSeededGlobalCycle = Utils.TriangularFunction(timePrevSeededGlobal
        //        , _gunTraverse, halfCycleTime, halfTravers);
        //    float angleCurSeededGlobalCycle = Utils.TriangularFunction(timeCurSeededGlobal
        //        , _gunTraverse, halfCycleTime, halfTravers);

        //    //float targetAngle = Utils.TriangularFunction(timeNextSeededGlobal + timeMove, _gunTraverse
        //    //    , halfCycleTime, halfTravers);

        //    float testAngle = Utils.TriangularFunction(timeCurSeededGlobal - 0.9f, _gunTraverse
        //        , halfCycleTime, halfTravers);


        //    (float, float) posibleTimeMovesPrev = Utils.CalculateS(CurrentGunRot
        //        , _gunTraverse, halfCycleTime, halfTravers, timePrevSeededGlobal);
        //    //float possibleTime1 = posibleTimeMovesPrev.Item1;
        //    //float possibleTime2 = posibleTimeMovesPrev.Item2;

        //    //float possibleTimeMove1Fixed = posibleTimeMovesPrev.Item1;
        //    //float possibleTimeMove2Fixed = posibleTimeMovesPrev.Item2;

        //    float possibleTimeMove1Fixed = GetFixedTime(posibleTimeMovesPrev.Item1);
        //    float possibleTimeMove2Fixed = GetFixedTime(posibleTimeMovesPrev.Item2);



        //    (float, float) posibleTimeMovesPrev3 = Utils.CalculateS(-CurrentGunRot
        //        , _gunTraverse, halfCycleTime, halfTravers, timePrevSeededGlobal);
        //    //float possibleTime1 = posibleTimeMovesPrev.Item1;
        //    //float possibleTime2 = posibleTimeMovesPrev.Item2;

        //    //float possibleTimeMove1Fixed = posibleTimeMovesPrev.Item1;
        //    //float possibleTimeMove2Fixed = posibleTimeMovesPrev.Item2;

        //    float possibleTimeMove1Fixed3 = GetFixedTime(posibleTimeMovesPrev3.Item1);
        //    float possibleTimeMove2Fixed3 = GetFixedTime(posibleTimeMovesPrev3.Item2);




        //    //(float, float) posibleTimeMovesPrev2 = Utils.CalculateS(CurrentGunRot
        //    //   , _gunTraverse, halfCycleTime, halfTravers, 0);
        //    //float possibleTime1 = posibleTimeMovesPrev2.Item1;
        //    //float possibleTime2 = posibleTimeMovesPrev2.Item2;
        //    //
        //    //float possibleTimeMove1Fixed2 = GetFixedTime(posibleTimeMovesPrev2.Item1);
        //    //float possibleTimeMove2Fixed2 = GetFixedTime(posibleTimeMovesPrev2.Item2);

        //    float targetMove;
        //    if (AreNumbersClose(
        //        ExtractScaledFraction(possibleTimeMove1Fixed - possibleTimeMove2Fixed),
        //        ExtractScaledFraction(possibleTimeMove1Fixed3), 0))
        //    {
        //        targetMove = possibleTimeMove1Fixed;
        //    }
        //    else
        //    {
        //        targetMove = possibleTimeMove2Fixed;
        //    }


        //    float targetAngle = Utils.TriangularFunction(timeCurSeededGlobal - possibleTimeMove1Fixed, _gunTraverse
        //        , halfCycleTime, halfTravers);


        //    Aim(testAngle, _gunTraverse, rotSpeed
        //        , _aimedAngle, false);


        //    //Debug.Log(targetTime);

        //    Debug.Log($" |+| {possibleTimeMove1Fixed.ToString("f3")} | {possibleTimeMove2Fixed.ToString("f3")}| |+| {(possibleTimeMove1Fixed3).ToString("f3")} | {(possibleTimeMove2Fixed3).ToString("f3")}| {GetFixedTime(timePrevSeededGlobal).ToString("f3")}");







        //    float GetFixedTime(float time)
        //    {
        //        time = time % fullCycleTime;
        //        time = Mathf.Abs(time);
        //        return time < halfCycleTime ? time : fullCycleTime - time;
        //    }


        //    bool AreNumbersClose(float x, float y, float dif)
        //    {
        //        dif = 0.002f;
        //        return Math.Abs(x - y) <= dif;
        //    }

        //}







        //(float, float) possibleTimesGlobal1 = Utils.CalculateS(angleCurSeededGlobalCycle
        //   , _gunTraverse, halfCycleTime, halfTravers, possibleTime1 + Time.deltaTime);
        //float possibleTime1Global1 = GetFixedTime(possibleTimesGlobal1.Item1);
        //float possibleTime2Global1 = GetFixedTime(possibleTimesGlobal1.Item2);


        //(float, float) possibleTimesGlobal2 = Utils.CalculateS(angleCurSeededGlobalCycle
        //   , _gunTraverse, halfCycleTime, halfTravers, possibleTime2 + Time.deltaTime);
        //float possibleTime1Global2 = GetFixedTime(possibleTimesGlobal2.Item1);
        //float possibleTime2Global2 = GetFixedTime(possibleTimesGlobal2.Item2);

        //Debug.Log($"{possibleTime1Global1.ToString("f3")} | {possibleTime2Global1.ToString("f3")}"
        //    + $" |+| {possibleTime1Global2.ToString("f3")}" + $" | {possibleTime2Global2.ToString("f3")}"
        //    + $" |+| {possibleTimeMove1Fixed.ToString("f3")} | {possibleTimeMove2Fixed.ToString("f3")}");





        //(float, float) posibleTimeMovesPrev = Utils.CalculateS(CurrentGunRot
        //        , _gunTraverse, halfCycleTime, halfTravers, timePreviousSeededGlobalCycle);
        //float timeToNextPrev1 = Mathf.Abs(posibleTimeMovesPrev.Item1);
        //float timeToNextPrev2 = Mathf.Abs(posibleTimeMovesPrev.Item2);

        //float timeToNextPrev1Fixed = timeToNextPrev1 < halfCycleTime ? timeToNextPrev1 : fullCycleTime - timeToNextPrev1;
        //float timeToNextPrev2Fixed = timeToNextPrev2 < halfCycleTime ? timeToNextPrev2 : fullCycleTime - timeToNextPrev2;


        //float angle1 = Utils.TriangularFunction(timeToNextPrev1Fixed, _gunTraverse
        //    , halfCycleTime, halfTravers);
        //float angle2 = Utils.TriangularFunction(timeToNextPrev2Fixed, _gunTraverse
        //    , halfCycleTime, halfTravers);


        //(float, float) posibleTimeMoves01 = Utils.CalculateS(angle1
        //    , _gunTraverse, halfCycleTime, halfTravers, 0);
        //float time011 = Mathf.Abs(posibleTimeMoves01.Item1);
        //float time012 = Mathf.Abs(posibleTimeMoves01.Item2);



        //(float, float) posibleTimeMoves02 = Utils.CalculateS(angle2
        //     , _gunTraverse, halfCycleTime, halfTravers, 0);
        //float time021 = Mathf.Abs(posibleTimeMoves02.Item1);
        //float time022 = Mathf.Abs(posibleTimeMoves02.Item2);






        //(float, float) posibleTimeMovesCur = Utils.CalculateS(CurrentGunRot
        //    , _gunTraverse, halfCycleTime, halfTravers, timeSeededGlobalCycle);
        //float timeToNextCur1 = Mathf.Abs(posibleTimeMovesCur.Item1);
        //float timeToNextCur2 = Mathf.Abs(posibleTimeMovesCur.Item2);

        //float timeToNextCur1Fixed = timeToNextCur1 < halfCycleTime ? timeToNextCur1 : fullCycleTime - timeToNextCur1;
        //float timeToNextCur2Fixed = timeToNextCur2 < halfCycleTime ? timeToNextCur2 : fullCycleTime - timeToNextCur2;


        //float targetTime;
        //if(timeToNextPrev1Fixed == timeToNextCur1Fixed)
        //{
        //    targetTime = timeToNextPrev1Fixed;
        //}
        //else if(timeToNextPrev1Fixed == timeToNextCur2Fixed)
        //{
        //    targetTime = timeToNextPrev1Fixed;
        //}
        //else if(timeToNextPrev2Fixed == timeToNextCur1Fixed)
        //{
        //    targetTime = timeToNextPrev2Fixed;
        //}
        //else if (timeToNextPrev2Fixed == timeToNextCur2Fixed)
        //{
        //    targetTime = timeToNextPrev2Fixed;
        //}
        //else
        //{
        //    targetTime = timeToNextPrev1Fixed;
        //    Debug.Log("ERROR");
        //}
















        ///float angleCurrentPlus = CurrentGunRot + _gunTraverse / 2;
        ///float angleSeededGlobalCyclePlus =
        ///    (angleSeededGlobalCycle - Time.deltaTime) + _gunTraverse / 2;
        ///
        ///float angleToNext1 = Mathf.Abs(angleCurrentPlus - angleSeededGlobalCyclePlus);
        ///float angleToNext2 = Mathf.Abs((angleFullCycle - angleCurrentPlus)
        ///    - angleSeededGlobalCyclePlus);
        ///
        ///
        ///float timeToNext1 = Utils.CalculateRotationTime(rotSpeed, angleToNext1);
        ///float timeToNext2 = Utils.CalculateRotationTime(rotSpeed, angleToNext2);

        //float angleSeededGlobalCycle = Utils.TriangularFunction(timeSeededGlobal, _gunTraverse
        //    , halfCycleTime, halfTravers);






        //float angle1 = Utils.TriangularFunction(timeToNext1Fixed, _gunTraverse
        //    , halfCycleTime, halfTravers);
        //float angle2 = Utils.TriangularFunction(timeToNext2Fixed, _gunTraverse
        //    , halfCycleTime, halfTravers);


        //(float, float) posibleTimeMoves01 = Utils.CalculateS(angle1
        //    , _gunTraverse, halfCycleTime, halfTravers, 0);
        //float time011 = Mathf.Abs(posibleTimeMoves01.Item1);
        //float time012 = Mathf.Abs(posibleTimeMoves01.Item2);



        //(float, float) posibleTimeMoves02 = Utils.CalculateS(angle2
        //     , _gunTraverse, halfCycleTime, halfTravers, 0);
        //float time021 = Mathf.Abs(posibleTimeMoves02.Item1);
        //float time022 = Mathf.Abs(posibleTimeMoves02.Item2);





        //Debug.Log($"{timeSeededGlobal} |{Time.time} | {Time.deltaTime}" +
        //    $" |{Time.frameCount}");



        //float angleToNextClose;
        //float angleToNextFar;
        //if (angleToNext1 < angleToNext2)
        //{
        //angleToNextClose = angleToNext1;
        //angleToNextFar = angleToNext2;
        //}
        //else if (angleToNext1 > angleToNext2)
        //{
        //angleToNextClose = angleToNext2;
        //angleToNextFar = angleToNext1;
        //}
        //else
        //{
        //Debug.Log("ERROR ==");
        //angleToNextClose = angleToNext1;
        //angleToNextFar = angleToNext2;
        //}




        //float timeToNext1Fixed = timeToNext1 < halfCycleTime ? timeToNext1 : fullCycleTime - timeToNext1;
        //float timeToNext2Fixed = timeToNext2 < halfCycleTime ? timeToNext2 : fullCycleTime - timeToNext2;




        //private void LostTargetActionSearch()
        //{
        //    float rotSpeed = _rotateSpeed * 0.25f;
        //    float angleFullCycle = _gunTraverse * 2;
        //    float halfTravers = _gunTraverse / 2;
        //    float fullCycleTime = Utils.CalculateRotationTime(rotSpeed, angleFullCycle);
        //    float halfCycleTime = fullCycleTime / 2;

        //    int seed = GetInstanceID() + _randomManager.Seed;
        //    System.Random random = new System.Random(seed);
        //    float randomFloat = (float)random.NextDouble(); //0 - 1
        //    float timeSeededGlobal = Time.time /*+ (randomFloat * fullCycleTime)*/;
        //    float timeNextSeededGlobal = (timeSeededGlobal + Time.deltaTime);
        //    //float timeSeededGlobalCycle = timeSeededGlobal % fullCycleTime;
        //    //float timeNextSeededGlobalCycle = (timeSeededGlobal + Time.deltaTime) % fullCycleTime;


        //    float angleSeededGlobalCycle = Utils.TriangularFunction(timeSeededGlobal, _gunTraverse
        //        , halfCycleTime, halfTravers);



        //    float angleCurrentPlus = CurrentGunRot + _gunTraverse / 2;
        //    float angleSeededGlobalCyclePlus =
        //        (angleSeededGlobalCycle - Time.deltaTime) + _gunTraverse / 2;

        //    float angleToNext1 = Mathf.Abs(angleCurrentPlus - angleSeededGlobalCyclePlus);
        //    float angleToNext2 = Mathf.Abs((angleFullCycle - angleCurrentPlus)
        //        - angleSeededGlobalCyclePlus);




        //    float angleToNextClose;
        //    float angleToNextFar;
        //    if (angleToNext1 < angleToNext2)
        //    {
        //        angleToNextClose = angleToNext1;
        //        angleToNextFar = angleToNext2;
        //    }
        //    else if (angleToNext1 > angleToNext2)
        //    {
        //        angleToNextClose = angleToNext2;
        //        angleToNextFar = angleToNext1;
        //    }
        //    else
        //    {
        //        Debug.Log("ERROR ==");
        //        angleToNextClose = angleToNext1;
        //        angleToNextFar = angleToNext2;
        //    }

        //    float timeToNext1 = Utils.CalculateRotationTime(rotSpeed, angleToNextFar);
        //    float timeToNext2 = Utils.CalculateRotationTime(rotSpeed, angleToNextClose);

        //    float timeNext1 = timeSeededGlobal + timeToNext1;
        //    float timeNext2 = timeSeededGlobal + timeToNext2;

        //    float timeNextNext1 = timeNext1 + Time.deltaTime;
        //    float timeNextNext2 = timeNext2 + Time.deltaTime;

        //    float angleNext1 = Utils.TriangularFunction(
        //        timeNextNext1, _gunTraverse, halfCycleTime, halfTravers);

        //    float angleNext2 = Utils.TriangularFunction(
        //        timeNextNext2, _gunTraverse, halfCycleTime, halfTravers);

        //    float angleSeededGlobalCycleNext = Utils.TriangularFunction(
        //        timeNextSeededGlobal, _gunTraverse, halfCycleTime, halfTravers);

        //    bool globalUp = angleSeededGlobalCycleNext > angleSeededGlobalCycle;
        //    bool Up1 = angleNext1 > CurrentGunRot;
        //    bool Up2 = angleNext2 > CurrentGunRot;

        //    float targetAngle;
        //    if (globalUp && Up1 && !Up2) //a
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("1-1");
        //    }
        //    else if (globalUp && Up1 && Up2) //b
        //    {
        //        if (angleNext1 > angleNext2)
        //        {
        //            targetAngle = angleNext1;
        //            Debug.Log("2-1");
        //        }
        //        else
        //        {
        //            targetAngle = angleNext2;
        //            Debug.Log("2-2");
        //        }
        //    }
        //    else if (globalUp && !Up1 && Up2) //c
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("3-1");
        //    }
        //    else if (!globalUp && !Up1 && Up2) //d
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("4-1");
        //    }
        //    else if (!globalUp && !Up1 && !Up2) //e
        //    {
        //        if (angleNext1 > angleNext2)
        //        {
        //            targetAngle = angleNext1;
        //            Debug.Log("5-1");
        //        }
        //        else
        //        {
        //            targetAngle = angleNext2;
        //            Debug.Log("5-2");
        //        }
        //    }
        //    else if (!globalUp && Up1 && !Up2) //f
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("6-1");
        //    }
        //    else
        //    {
        //        Debug.LogError("ERROR NOOOO");
        //        targetAngle = angleNext1;
        //    }


















        //    Debug.Log($"{targetAngle.ToString("f3")} | {CurrentGunRot.ToString("f3")}" +
        //        $"| {angleSeededGlobalCycleNext.ToString("f3")}");

        //    Aim(targetAngle, _gunTraverse, rotSpeed
        //        , _aimedAngle, false);
        //}





        //private void LostTargetActionSearch()
        //{
        //    float rotSpeed = _rotateSpeed * 0.25f;
        //    float angleFullCycle = _gunTraverse * 2;
        //    float halfTravers = _gunTraverse / 2;
        //    float fullCycleTime = Utils.CalculateRotationTime(rotSpeed, angleFullCycle);
        //    float halfCycleTime = fullCycleTime / 2;

        //    int seed = GetInstanceID() + _randomManager.Seed;
        //    System.Random random = new System.Random(seed);
        //    float randomFloat = (float)random.NextDouble(); //0 - 1
        //    float timeSeededGlobal = Time.time /*+ (randomFloat * fullCycleTime)*/;
        //    float timeNextSeededGlobal = (timeSeededGlobal + Time.deltaTime);
        //    float timePreviousSeededGlobal = (timeSeededGlobal - Time.deltaTime);
        //    //float timeSeededGlobalCycle = timeSeededGlobal % fullCycleTime;
        //    //float timeNextSeededGlobalCycle = (timeSeededGlobal + Time.deltaTime) % fullCycleTime;


        //    float angleSeededGlobalCycle = Utils.TriangularFunction(timePreviousSeededGlobal, _gunTraverse
        //        , halfCycleTime, halfTravers);



        //    float angleCurrentPlus = CurrentGunRot + _gunTraverse / 2;
        //    float angleSeededGlobalCyclePlus =
        //        (angleSeededGlobalCycle - Time.deltaTime) + _gunTraverse / 2;

        //    float angleToNext1 = Mathf.Abs(angleCurrentPlus - angleSeededGlobalCyclePlus);
        //    float angleToNext2 = Mathf.Abs((angleFullCycle - angleCurrentPlus)
        //        - angleSeededGlobalCyclePlus);




        //    float angleToNextClose;
        //    float angleToNextFar;
        //    if (angleToNext1 < angleToNext2)
        //    {
        //        angleToNextClose = angleToNext1;
        //        angleToNextFar = angleToNext2;
        //    }
        //    else if (angleToNext1 > angleToNext2)
        //    {
        //        angleToNextClose = angleToNext2;
        //        angleToNextFar = angleToNext1;
        //    }
        //    else
        //    {
        //        Debug.Log("ERROR ==");
        //        angleToNextClose = angleToNext1;
        //        angleToNextFar = angleToNext2;
        //    }

        //    float timeToNext1 = Utils.CalculateRotationTime(rotSpeed, angleToNext1);
        //    float timeToNext2 = Utils.CalculateRotationTime(rotSpeed, angleToNext2);

        //    float timeNext1 = timePreviousSeededGlobal + timeToNext1;
        //    float timeNext2 = timePreviousSeededGlobal + timeToNext2;

        //    float timeNextNext1 = timeNext1 + Time.deltaTime;
        //    float timeNextNext2 = timeNext2 + Time.deltaTime;

        //    float angleNext1 = Utils.TriangularFunction(
        //        timeNextNext1, _gunTraverse, halfCycleTime, halfTravers);

        //    float angleNext2 = Utils.TriangularFunction(
        //        timeNextNext2, _gunTraverse, halfCycleTime, halfTravers);

        //    float angleSeededGlobalCycleNext = Utils.TriangularFunction(
        //        timeSeededGlobal, _gunTraverse, halfCycleTime, halfTravers);

        //    bool globalUp = angleSeededGlobalCycleNext > angleSeededGlobalCycle;
        //    bool Up1 = Utils.IsTriangularFunctionRising(timeNext1
        //        , _gunTraverse, halfCycleTime, halfTravers);
        //    bool Up2 = Utils.IsTriangularFunctionRising(timeNext2
        //        , _gunTraverse, halfCycleTime, halfTravers);

        //    float targetAngle;
        //    if (globalUp && Up1 && !Up2) //a
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("1-1");
        //    }
        //    else if (globalUp && Up1 && Up2) //b
        //    {
        //        if (angleNext1 > angleNext2)
        //        {
        //            targetAngle = angleNext1;
        //            Debug.Log("2-1");
        //        }
        //        else
        //        {
        //            targetAngle = angleNext2;
        //            Debug.Log("2-2");
        //        }
        //    }
        //    else if (globalUp && !Up1 && Up2) //c
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("3-1");
        //    }
        //    else if (!globalUp && !Up1 && Up2) //d
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("4-1");
        //    }
        //    else if (!globalUp && !Up1 && !Up2) //e
        //    {
        //        if (angleNext1 > angleNext2)
        //        {
        //            targetAngle = angleNext1;
        //            Debug.Log("5-1");
        //        }
        //        else
        //        {
        //            targetAngle = angleNext2;
        //            Debug.Log("5-2");
        //        }
        //    }
        //    else if (!globalUp && Up1 && !Up2) //f
        //    {
        //        targetAngle = angleNext1;
        //        Debug.Log("6-1");
        //    }
        //    else
        //    {
        //        Debug.LogError("ERROR NOOOO");
        //        targetAngle = angleNext1;
        //    }


















        //    //Debug.Log($"{targetAngle.ToString("f3")} | {CurrentGunRot.ToString("f3")}" +
        //    //    $"| {angleSeededGlobalCycle.ToString("f3")}");

        //    Aim(targetAngle, _gunTraverse, rotSpeed
        //        , _aimedAngle, false);
        //}









        //private void LostTargetActionSearch()
        //{
        //    float rotSpeed = _rotateSpeed * 0.25f;
        //    float fullCycleAngle = _gunTraverse * 2;
        //    float halfTravers = _gunTraverse / 2;
        //    float fullCycleTime = Utils.CalculateRotationTime(rotSpeed, fullCycleAngle);
        //    float halfCycleTime = fullCycleTime / 2;

        //    int seed = GetInstanceID() + _randomManager.Seed;
        //    System.Random random = new System.Random(seed);
        //    float randomFloat = (float)random.NextDouble(); //0 - 1
        //    float timeSeededGlobal = Time.time /*+ (randomFloat * fullCycleTime)*/;
        //    float timeNextSeededGlobal = (timeSeededGlobal + Time.deltaTime);
        //    //float timeSeededGlobalCycle = timeSeededGlobal % fullCycleTime;
        //    //float timeNextSeededGlobalCycle = (timeSeededGlobal + Time.deltaTime) % fullCycleTime;

        //    float angleSeededGlobalCycle = Utils.GetYValue(timeSeededGlobal,
        //        -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);
        //    float angleNextSeededGlobalCycle = Utils.GetYValue(timeNextSeededGlobal,
        //        -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);

        //    (float, float) distances = Utils.GetXByYClosestTwoDist(timeSeededGlobal, CurrentGunRot
        //        , -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);
        //    bool isGlobalGoUp = angleNextSeededGlobalCycle >= angleSeededGlobalCycle;
        //    if (angleNextSeededGlobalCycle == angleSeededGlobalCycle)
        //    {
        //        Debug.LogError("Error");
        //    }


        //    float distance1 = distances.Item1;
        //    float distance2 = distances.Item2;

        //    float angleDistance1 = Utils.GetYValue(timeSeededGlobal + distance1,
        //        -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);
        //    float angleNextDistance1 = Utils.GetYValue(timeSeededGlobal + distance1 + Time.deltaTime,
        //        -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);
        //    bool isDistance1GoUp = angleDistance1 >= angleNextDistance1;

        //    float angleDistance2 = Utils.GetYValue(timeSeededGlobal + distance2,
        //        -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);
        //    float angleNextDistance2 = Utils.GetYValue(timeSeededGlobal + distance2 + Time.deltaTime,
        //        -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);
        //    bool isDistance2GoUp = angleDistance2 >= angleNextDistance2;

        //    float targetAngle;
        //    if (isGlobalGoUp)
        //    {
        //        if (angleNextDistance1 > angleNextDistance2)
        //        {
        //            targetAngle = angleDistance1;
        //        }
        //        else if (angleNextDistance1 < angleNextDistance2)
        //        {
        //            targetAngle = angleDistance2;
        //        }
        //        else
        //        {
        //            targetAngle = angleDistance1;
        //            Debug.LogError("Error");
        //        }
        //    }
        //    else
        //    {
        //        if (angleNextDistance1 < angleNextDistance2)
        //        {
        //            targetAngle = angleDistance1;
        //        }
        //        else if (angleNextDistance1 > angleNextDistance2)
        //        {
        //            targetAngle = angleDistance2;
        //        }
        //        else
        //        {
        //            targetAngle = angleDistance1;
        //            Debug.LogError("Error");
        //        }
        //    }



        //    //Debug.Log($" {angleNextDistance1})



        //    //float targetTime = timeSeededGlobal + targetTimeMove;

        //    //float timeMove = Utils.GetXByYClosestTo(targetTime, CurrentGunRot,
        //    //    -_gunTraverse / 2, _gunTraverse / 2, fullCycleTime);


        //    //Debug.Log($"{targetAngle} | {CurrentGunRot} | {timeMove}");

        //    Aim(targetAngle, _gunTraverse, rotSpeed
        //        , _aimedAngle, false);
        //}





        //private void LostTargetActionSearch()
        //{
        //    float rotSpeed = _rotateSpeed * 0.25f;
        //    float fullCycleAngle = _gunTraverse * 2;
        //    float halfTravers = _gunTraverse / 2;
        //    float fullCycleTime = Utils.CalculateRotationTime(rotSpeed, fullCycleAngle);
        //    float halfCycleTime = fullCycleTime / 2;

        //    int seed = GetInstanceID() + _randomManager.Seed;
        //    System.Random random = new System.Random(seed);
        //    float randomFloat = (float)random.NextDouble(); //0 - 1
        //    float timeSeededGlobal = Time.time /*+ (randomFloat * fullCycleTime)*/;
        //    float timeSeededGlobalCycle = timeSeededGlobal % fullCycleTime;
        //    float angleSeededGlobalCycle = timeSeededGlobalCycle / fullCycleTime * 360.0f;







        //    float angleSeededGlobalCycleHalf = angleSeededGlobalCycle / 2;




        //    Debug.Log(angleSeededGlobalCycle);


        //    float currentGunRotPlus = CurrentGunRot + halfTravers;

        //    float angleCycleFirstHalf = currentGunRotPlus;
        //    float angleCycleSecondHalf = fullCycleAngle - currentGunRotPlus;

        //    float timeCycleFirstHalf = Utils.CalculateRotationTime(rotSpeed, angleCycleFirstHalf);
        //    float timeCycleSecondHalf = Utils.CalculateRotationTime(rotSpeed, angleCycleSecondHalf);

        //    float timeCycleFirstHalfMod = timeCycleFirstHalf + Time.deltaTime;
        //    //float timeCycleSecondHalfMod = timeCycleSecondHalf + Time.deltaTime;

        //    float targetCycleTime;

        //    ///float twoOneDiff = Math.Abs(timeCycleSecondHalf - timeCycleFirstHalf);
        //    ///float globalOneDiff = Math.Abs(timeSeededGlobalCycle - timeCycleFirstHalf);
        //    ///float globalTwoDiff = Math.Abs(timeSeededGlobalCycle - timeCycleSecondHalf);

        //    if (timeSeededGlobalCycle < halfCycleTime)
        //    {
        //        float diff = timeCycleFirstHalf - timeSeededGlobalCycle;

        //        targetCycleTime = timeSeededGlobalCycle + diff;
        //    }
        //    else
        //    {
        //        float diff = timeCycleSecondHalf - timeSeededGlobalCycle;

        //        targetCycleTime = timeSeededGlobalCycle + diff;
        //    }

        //    targetCycleTime = timeSeededGlobalCycle + halfCycleTime / 2;

        //    //float nextCycleTime = (targetCycleTime + Time.deltaTime) % fullCycleTime;
        //    float nextCycleTime = targetCycleTime % fullCycleTime;
        //    float side = Mathf.Sign((fullCycleTime / 2) - nextCycleTime);

        //    Debug.Log($"{targetCycleTime.ToString("f2")} | " +
        //        $"{timeSeededGlobalCycle.ToString("f2")}");

        //    //Debug.Log($"{side} + {targetCycleTime}");
        //    //Debug.Log($"{(int)nextCycleTime} | {(int)seededGlobalCycleTime}");

        //    Aim(_gunTraverse / 2 * side, _gunTraverse, rotSpeed
        //       , _aimedAngle, false);
        //}






        //float timeCycleFirstHalf = Utils.CalculateRotationTime(rotSpeed, angleCycleFirstHalf);
        //float timeCycleSecondHalf = Utils.CalculateRotationTime(rotSpeed, angleCycleSecondHalf);

        //float timeCycleFirstHalfMod = timeCycleFirstHalf + Time.deltaTime;
        //float timeCycleSecondHalfMod = timeCycleSecondHalf + Time.deltaTime;

        //float targetCycleTime;

        //if (seededGlobalCycleTime < )
        //{
        //    if()

        //    targetCycleTime = timeCycleFirstHalfMod;
        //    Debug.Log("1");

        //}
        //else if (seededGlobalTime < timeCycleSecondHalfMod)
        //{
        //    targetCycleTime = timeCycleSecondHalfMod;
        //    Debug.Log("3");
        //}
        //else if (seededGlobalCycleTime >= timeCycleFirstHalfMod && seededGlobalTime >= timeCycleSecondHalfMod)
        //{
        //    targetCycleTime = timeCycleFirstHalfMod;
        //    Debug.Log("5");
        //}
        //else
        //{
        //    Debug.LogError("Cycle error");
        //    targetCycleTime = 0;
        //}





        //float currentGunRotPlus = CurrentGunRot + halfTravers;
        //float angleCycleFirstHalf = currentGunRotPlus;

        //float timeCycleFirstHalf = Utils.CalculateRotationTime(rotSpeed, angleCycleFirstHalf);

        //float timeCycleFirstHalfMod = timeCycleFirstHalf + Time.deltaTime;

        //float targetCycleTime;
        //if (seededGlobalCycleTime < timeCycleFirstHalfMod)
        //{
        //    targetCycleTime = timeCycleFirstHalfMod;
        //}
        //else
        //{
        //    targetCycleTime = timeCycleFirstHalfMod;
        //}





        //if(seededGlobalCycleTime < timeCycleFirstHalf)
        //{
        //    if(timeCycleFirstHalf + Time.deltaTime < timeCycleSecondHalf)
        //    {
        //        targetCycleTime = timeCycleFirstHalf;
        //        Debug.Log("1");
        //    }
        //    else
        //    {
        //        targetCycleTime = timeCycleSecondHalf;
        //        Debug.Log("2");
        //    }

        //}
        //else if(seededGlobalTime < timeCycleSecondHalf)
        //{
        //    targetCycleTime = timeCycleSecondHalf;
        //    Debug.Log("3");
        //}
        //else if (seededGlobalCycleTime >= timeCycleFirstHalf && seededGlobalTime >= timeCycleSecondHalf)
        //{
        //    targetCycleTime = timeCycleFirstHalf;
        //    Debug.Log("5");
        //}
        //else
        //{
        //    Debug.LogError("Cycle error");
        //    targetCycleTime = 0;
        //}





        //float startSide = Mathf.Sign((fullCycleTime / 2) - seededGlobalCycleTime);


        //float currentCycleAngle = (startSide * -_gunTraverse + _gunTraverse) + (startSide * currentGunRotPlus);

        //float cycleTime = Utils.CalculateRotationTime(rotSpeed, currentCycleAngle);
        //float nextCycleTime = (cycleTime + Time.deltaTime) % fullCycleAngle;

        //float side = Mathf.Sign((fullCycleTime / 2) - nextCycleTime);




        //float currentCycleAngle;
        //if (side > 0)
        //{
        //    currentCycleAngle = currentGunRotNoMinus;
        //}
        //else
        //{
        //    currentCycleAngle = _gunTraverse + _gunTraverse - currentGunRotNoMinus;
        //}



        //private void LostTargetActionSearch()
        //{
        //    float rotSpeed = _rotateSpeed * 0.25f;
        //    //float angleDistance = Mathf.DeltaAngle()


        //    float rotTime = Utils.CalculateRotationTime(rotSpeed, _gunTraverse);


        //    int seed = GetInstanceID() + _randomManager.Seed;
        //    System.Random random = new System.Random(seed);
        //    float randomFloat = (float)random.NextDouble();
        //    float sinMove = randomFloat * rotTime;



        //    float value = Mathf.Sin((Time.time + sinMove) * Mathf.PI * (1 / rotTime));
        //    Aim(_gunTraverse / 2 * Mathf.Sign(value), _gunTraverse, rotSpeed
        //       , _aimedAngle, false);
        //}

        private void LostTargetActionSearch2()
        {
            float rotSpeed = _rotateSpeed * 0.25f;

            float targetRot = _gunTraverse / 2;

            if (Mathf.Abs(CurrentGunRot) >= Mathf.Abs(targetRot))
            {
                Debug.Log("swap");
                _goLeftSearch = !_goLeftSearch;
            }

            Aim(targetRot * (_goLeftSearch ? 1 : -1), _gunTraverse, rotSpeed
                , _aimedAngle, false);
        }

        private void OnEnable()
        {
            StartAimingAt(_playerManager.PlayerBody.transform);
            StartShooting();
        }
    }
}

