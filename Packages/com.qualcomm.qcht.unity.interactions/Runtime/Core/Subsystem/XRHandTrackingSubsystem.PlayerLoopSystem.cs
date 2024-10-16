// /******************************************************************************
//  * File: XRHandTrackingSubsystem.PlayerLoopSystem.cs
//  * Copyright (c) 2023 Qualcomm Technologies, Inc. and/or its subsidiaries. All rights reserved.
//  *
//  * Confidential and Proprietary - Qualcomm Technologies, Inc.
//  *
//  ******************************************************************************/

using System;
using System.Linq;
using UnityEngine.LowLevel;
using UnityEngine.XR.Interaction.Toolkit;

namespace QCHT.Interactions.Core
{
    public partial class XRHandTrackingSubsystem
    {
        /// <summary>
        /// XR Hand tracking player loop system.
        /// </summary>
        private struct XRHandTrackingLoopSystem
        {
        }

        /// <summary>
        /// Tries to add update delegate for hand tracking subsystem through a player loop system at XRUpdate step.
        /// Custom player loop system for hand tracking will be added to current player loop if does not exist yet.
        /// </summary>
        private void AddPlayerLoopSystem()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!TryFindPlayerLoopSystem(playerLoop, typeof(UnityEngine.PlayerLoop.EarlyUpdate), out var earlyIndex))
                return;

            if (!TryFindPlayerLoopSystem(playerLoop.subSystemList[earlyIndex],
                    typeof(UnityEngine.PlayerLoop.EarlyUpdate.XRUpdate), out var xrIndex))
                return;

            var loopSystem = playerLoop.subSystemList[earlyIndex].subSystemList[xrIndex];
            if (TryFindPlayerLoopSystem(loopSystem, typeof(XRHandTrackingLoopSystem), out _))
                return; // already exists

            loopSystem.subSystemList ??= new PlayerLoopSystem[] { };

            var list = loopSystem.subSystemList.ToList();
            list.Add(new PlayerLoopSystem
            {
                type = typeof(XRHandTrackingLoopSystem),
                updateDelegate = OnUpdate
            });
            playerLoop.subSystemList[earlyIndex].subSystemList[xrIndex].subSystemList = list.ToArray();

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        /// <summary>
        /// Removes custom player loop system for hand tracking from current player loop.
        /// </summary>
        private void RemovePlayerLoopSystem()
        {
            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            if (!TryFindPlayerLoopSystem(playerLoop, typeof(UnityEngine.PlayerLoop.EarlyUpdate), out var earlyIndex))
                return;

            if (!TryFindPlayerLoopSystem(playerLoop.subSystemList[earlyIndex],
                    typeof(UnityEngine.PlayerLoop.EarlyUpdate.XRUpdate), out var xrIndex))
                return;

            var loopSystem = playerLoop.subSystemList[earlyIndex].subSystemList[xrIndex];
            if (!TryFindPlayerLoopSystem(loopSystem, typeof(XRHandTrackingLoopSystem), out _))
                return;

            var list = loopSystem.subSystemList.ToList();
            list.RemoveAll(x => x.type == typeof(XRHandTrackingLoopSystem));
            playerLoop.subSystemList[earlyIndex].subSystemList[xrIndex].subSystemList = list.ToArray();

            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        /// <summary>
        /// Finds player loop system in player system list by looking for type.
        /// </summary>
        private static bool TryFindPlayerLoopSystem(PlayerLoopSystem playerLoop, Type type, out int index)
        {
            if (playerLoop.subSystemList == null)
            {
                index = -1;
                return false;
            }
            
            for (var i = 0; i < playerLoop.subSystemList.Length; i++)
            {
                if (playerLoop.subSystemList[i].type != type) continue;
                index = i;
                return true;
            }

            index = -1;
            return false;
        }

        /// <summary>
        /// Player loop system delegate function.
        /// </summary>
        private void OnUpdate() => UpdateHands(XRInteractionUpdateOrder.UpdatePhase.Dynamic);
    }
}