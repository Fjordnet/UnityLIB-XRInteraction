using System.Collections;
using System.Collections.Generic;
using Fjord.Common.Types;
using UnityEngine;
#if !UNITY_EDITOR && UNITY_WSA
using System;
using System.Collections.Generic;
using Windows.Devices.Haptics;
using Windows.Foundation;
using Windows.Perception;
using Windows.Storage.Streams;
using Windows.UI.Input.Spatial;
using UnityEngine.XR.WSA.Input;
#endif

namespace Fjord.XRInteraction.XRHaptics
{
    public class WMRHapticEngine : XRHapticEngine
    {
        private const ushort ContinuousBuzzWaveform = 0x1004;
        private uint _interactionSourceId;

        public override void Initialize(Chirality chirality)
        {
#if !UNITY_EDITOR && UNITY_2017_2_OR_NEWER && UNITY_WSA
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
                IReadOnlyList<SpatialInteractionSourceState> sources = SpatialInteractionManager
                                                                        .GetForCurrentView()
                                                                        .GetDetectedSourcesAtTimestamp(PerceptionTimestampHelper
                                                                        .FromHistoricalTargetTime(DateTimeOffset
                                                                        .Now));

                foreach (SpatialInteractionSourceState sourceState in sources)
                {
                    if (sourceState.Source.Handedness == SpatialInteractionSourceHandedness.Left && chirality == Chirality.Left)
                    {
                        _interactionSourceId = sourceState.Source.Id;
                    }
                    if (sourceState.Source.Handedness == SpatialInteractionSourceHandedness.Right && chirality == Chirality.Right)
                    {
                        _interactionSourceId = sourceState.Source.Id;
                    }
                }
            }, true);
#endif
        }

        public override void StartHaptics(float normalizedIntensity, float durationInSeconds)
        {
#if !UNITY_EDITOR && UNITY_2017_2_OR_NEWER && UNITY_WSA
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
                IReadOnlyList<SpatialInteractionSourceState> sources = SpatialInteractionManager
                                                                        .GetForCurrentView()
                                                                        .GetDetectedSourcesAtTimestamp(PerceptionTimestampHelper
                                                                        .FromHistoricalTargetTime(DateTimeOffset
                                                                        .Now));

                foreach (SpatialInteractionSourceState sourceState in sources)
                {
                    if (sourceState.Source.Id.Equals(_interactionSourceId))
                    {
                        SimpleHapticsController simpleHapticsController = sourceState
                                                                            .Source
                                                                            .Controller
                                                                            .SimpleHapticsController;
    
                        foreach (SimpleHapticsControllerFeedback hapticsFeedback in simpleHapticsController.SupportedFeedback)
                        {
                            if (hapticsFeedback.Waveform.Equals(ContinuousBuzzWaveform))
                            {
                                if (durationInSeconds.Equals(float.MaxValue))
                                {
                                    simpleHapticsController.SendHapticFeedback(hapticsFeedback, intensity);
                                }
                                else
                                {
                                    simpleHapticsController.SendHapticFeedbackForDuration(
                                                            hapticsFeedback, 
                                                            intensity, 
                                                            TimeSpan.FromSeconds(durationInSeconds));
                                }
                                return;
                            }
                        }
                    }
                }
            }, true);
#endif
        }

        public override void StopHaptics()
        {
#if !UNITY_EDITOR && UNITY_2017_2_OR_NEWER && UNITY_WSA
            UnityEngine.WSA.Application.InvokeOnUIThread(() =>
            {
                IReadOnlyList<SpatialInteractionSourceState> sources = SpatialInteractionManager
                                                                        .GetForCurrentView()
                                                                        .GetDetectedSourcesAtTimestamp(PerceptionTimestampHelper
                                                                        .FromHistoricalTargetTime(DateTimeOffset
                                                                        .Now));

                foreach (SpatialInteractionSourceState sourceState in sources)
                {
                    if (sourceState.Source.Id.Equals(_interactionSourceId))
                    {
                        sourceState.Source.Controller.SimpleHapticsController.StopFeedback();
                    }
                }
            }, true);
#endif
        }
    }
}