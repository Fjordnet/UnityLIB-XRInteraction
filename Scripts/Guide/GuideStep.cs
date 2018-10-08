using System.Collections;
using System.Collections.Generic;
using Fjord.Common.UnityEvents;
using UnityEngine;
using UnityEngine.Events;

namespace Fjord.XRInteraction.Attachables
{
    public class GuideStep : MonoBehaviour
    {
        [SerializeField]
        private string _stepName;

        [SerializeField]
        private UnityEvent _stepActivated;

        [SerializeField]
        private GameObjectUnityEvent _stepCompleted;

        [SerializeField]
        protected AudioClip _activateSound;

        private readonly Dictionary<string, float> _analytics = new Dictionary<string, float>();
        private float _stepActiveTime;
        private GuideSequence _guideSequence;

        public bool Activated { get; private set; }
        public bool Completed { get; private set; }

        public GameObjectUnityEvent StepCompleted
        {
            get { return _stepCompleted; }
        }

        public UnityEvent StepActivated
        {
            get { return _stepActivated; }
        }

        public Dictionary<string, float> Analytics
        {
            get { return _analytics; }
        }

        public virtual void Intialize(GuideSequence guideSequence, int index)
        {
//            _analytics.Add("attachment_speed", Random.Range(0f, 5f));
//            _analytics.Add("attachment_accuracy", Random.Range(0f, 5f));
//            _analytics.Add("drops", Random.Range(0f, 5f));
        }

        public virtual void OnStepActivated()
        {
            _stepActiveTime = Time.time;
            Activated = true;
            _stepActivated.Invoke();
        }

        public virtual void OnStepCompleted()
        {
            float time = Mathf.Clamp((Time.time - _stepActiveTime) / 5f, 0, 5);
            _analytics.Add("attachment_speed", time);
            Debug.Log("attachment_speed  " + time);
            Activated = false;
            Completed = true;
            _stepCompleted.Invoke(gameObject);
        }

        public virtual void DrawDizmoFromSequence(GuideSequence sequence, int index)
        {
        }
        
        public void DrawPulsingHighlight(Material highlightMaterial, MeshFilter[] meshFilters)
        {
            highlightMaterial.SetFloat("_RimPower", Mathf.PingPong(Time.time / 4f, .2f) + 1.2f);
            DrawOverlay(highlightMaterial, meshFilters);
        }

        public void DrawOverlay(Material highlightMaterial, MeshFilter[] meshFilters)
        {
            Matrix4x4 matrix4X4 = new Matrix4x4();
            for (int i = 0; i < meshFilters.Length; ++i)
            {
                if (meshFilters[i].gameObject.activeSelf)
                {
                    matrix4X4.SetTRS(
                        meshFilters[i].transform.position,
                        meshFilters[i].transform.rotation,
                        meshFilters[i].transform.lossyScale);

                    for (int si = 0; si < meshFilters[i].sharedMesh.subMeshCount; ++si)
                    {
                        Graphics.DrawMesh(
                            meshFilters[i].sharedMesh,
                            matrix4X4, highlightMaterial,
                            0,
                            null,
                            si,
                            null,
                            false,
                            false,
                            false);
                    }
                }
            }
        }

        protected void BounceAboveRenderers(MeshRenderer[] renderers, Transform transformToBounce)
        {
            Bounds bounds = new Bounds();
            bounds.center = renderers[0].bounds.center;
            bounds.extents = renderers[0].bounds.extents;
            for (int i = 1; i < renderers.Length; ++i)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            Vector3 abovePoint = bounds.center;
            abovePoint.y += 1000f;
            Vector3 placePoint = bounds.ClosestPoint(abovePoint);
            Vector3 highPlacePoint = placePoint;
            highPlacePoint.y += .05f;
            transformToBounce.transform.position = Vector3.Lerp(
                placePoint,
                highPlacePoint,
                Mathf.PingPong(Time.time, 1f));
        }

        protected void BounceRotation(Transform rotateAround, Transform transformToRotate)
        {
            transformToRotate.position = rotateAround.transform.position;
            transformToRotate.up = rotateAround.transform.up;
            transformToRotate.Rotate(0, Mathf.PingPong(Time.time, 1f) * 45, 0, Space.Self);
        }
    }
}