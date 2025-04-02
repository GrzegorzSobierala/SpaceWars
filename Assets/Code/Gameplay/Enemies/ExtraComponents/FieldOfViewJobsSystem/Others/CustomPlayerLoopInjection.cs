using System;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
using UnityEngine.Profiling;

namespace Game.Physics
{
    public class CustomPlayerLoopInjection
    {
        // Existing Action for PostLateUpdate
        public static Action OnPostLateUpdateEnd;
        // New Action for after Physics2DUpdate in PreUpdate phase
        public static Action OnAfterPhysics2DUpdate;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeCustomLoop()
        {
            // Get the current player loop.
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            // --- Modify PostLateUpdate ---
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(PostLateUpdate))
                {
                    var postLateUpdateSystems = playerLoop.subSystemList[i].subSystemList;
                    int insertIndex = -1;

                    // Find the index after ParticleSystemEndUpdateAll.
                    for (int j = 0; j < postLateUpdateSystems.Length; j++)
                    {
                        if (postLateUpdateSystems[j].type == typeof(PostLateUpdate.ParticleSystemEndUpdateAll))
                        {
                            insertIndex = j + 1;
                            break;
                        }
                    }

                    if (insertIndex < 0)
                    {
                        Debug.LogWarning("ParticleSystemEndUpdateAll not found; appending custom system at the end of PostLateUpdate.");
                        insertIndex = postLateUpdateSystems.Length;
                    }

                    var myPostLateUpdateSystem = new PlayerLoopSystem
                    {
                        type = typeof(CustomPlayerLoopInjection),
                        updateDelegate = PostLateUpdateEnd
                    };

                    var newPostLateUpdateSystems = new PlayerLoopSystem[postLateUpdateSystems.Length + 1];

                    for (int j = 0; j < insertIndex; j++)
                        newPostLateUpdateSystems[j] = postLateUpdateSystems[j];

                    newPostLateUpdateSystems[insertIndex] = myPostLateUpdateSystem;

                    for (int j = insertIndex; j < postLateUpdateSystems.Length; j++)
                        newPostLateUpdateSystems[j + 1] = postLateUpdateSystems[j];

                    playerLoop.subSystemList[i].subSystemList = newPostLateUpdateSystems;
                    break; // Found and modified PostLateUpdate, exit loop.
                }
            }

            // --- Modify PreUpdate to inject after Physics2DUpdate ---
            for (int i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type == typeof(PreUpdate))
                {
                    var preUpdateSystems = playerLoop.subSystemList[i].subSystemList;
                    int insertIndex = -1;

                    // Find the index after PreUpdate.Physics2DUpdate.
                    for (int j = 0; j < preUpdateSystems.Length; j++)
                    {
                        if (preUpdateSystems[j].type == typeof(PreUpdate.Physics2DUpdate))
                        {
                            insertIndex = j + 1;
                            break;
                        }
                    }

                    if (insertIndex < 0)
                    {
                        Debug.LogWarning("PreUpdate.Physics2DUpdate not found; appending custom system at the end of PreUpdate.");
                        insertIndex = preUpdateSystems.Length;
                    }

                    var myPreUpdateSystem = new PlayerLoopSystem
                    {
                        type = typeof(CustomPlayerLoopInjection),
                        updateDelegate = AfterPhysics2DUpdate
                    };

                    var newPreUpdateSystems = new PlayerLoopSystem[preUpdateSystems.Length + 1];

                    for (int j = 0; j < insertIndex; j++)
                        newPreUpdateSystems[j] = preUpdateSystems[j];

                    newPreUpdateSystems[insertIndex] = myPreUpdateSystem;

                    for (int j = insertIndex; j < preUpdateSystems.Length; j++)
                        newPreUpdateSystems[j + 1] = preUpdateSystems[j];

                    playerLoop.subSystemList[i].subSystemList = newPreUpdateSystems;
                    break; // Found and modified PreUpdate, exit loop.
                }
            }

            // Apply the modified player loop.
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        // Callback for PostLateUpdate injection.
        private static void PostLateUpdateEnd()
        {
            OnPostLateUpdateEnd?.Invoke();
        }

        // Callback for after Physics2DUpdate injection.
        private static void AfterPhysics2DUpdate()
        {
            Profiler.BeginSample("AfterPhysics2DUpdate");
            OnAfterPhysics2DUpdate?.Invoke();
            Profiler.EndSample();
        }
    }
}
