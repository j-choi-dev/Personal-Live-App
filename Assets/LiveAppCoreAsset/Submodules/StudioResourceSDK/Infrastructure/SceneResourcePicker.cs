using Live2D.Cubism.Core;
using Live2D.Cubism.Rendering;
using LiveAppCore;
using StudioCharacterSDK.Domain;
using StudioCommonSDK.Domain;
using StudioResourceSDK.Domain;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zenject;

namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾는 infra
    /// 현재는 캐릭터만 지원 @OH 26.11.04
    /// </summary>
    public class SceneResourcePicker : IResourcePickDomain
    {
        private readonly ISpawnPivotTransform _objectPivot;
        private const float _flatBoundsThickness = 0.001f;
        /// <summary>
        /// 로컬 범위의 꼭짓점 8개를 담는 버퍼. 클릭마다 배열을 새로 만들지 않도록 재사용
        /// </summary>
        private static readonly Vector3[] _corners = new Vector3[8];
        /// <summary>
        /// 대체 판정 경고를 이미 출력한 캐릭터 id. 클릭할 때마다 같은 경고가 쌓이지 않도록 하기 위함
        /// </summary>
        private readonly HashSet<string> _alreadyWarnedIds = new HashSet<string>();

        public SceneResourcePicker(
            [Inject( Id = SpawnPivotId.Object)]
            ISpawnPivotTransform objectPivot )
        {
            _objectPivot = objectPivot;
        }

        public (ResourceType type, string id)? Pick(ViewportPoint point)
        {
            // 생성자로 카메라 할당 시 생성 시점에는 카메라가 없을 수 있어서 피킹할 때 가져옮
            Camera renderCamera = Camera.main;
            if(renderCamera == null)
            {
                Debug.LogWarning("[SceneResourcePicker] MainCamera 태그가 붙은 카메라가 없습니다.");
                return null;
            }

            if(_objectPivot == null || _objectPivot.Transform == null)
            {
                Debug.LogWarning("[SceneResourcePicker] 스폰 지점이 없습니다.");
                return null;
            }

            // viewport 좌표 -> 렌더 카메라 기준 월드 ray
            Ray ray = renderCamera.ViewportPointToRay(new Vector3(point.X, point.Y, 0f));

            string frontId = null;
            int frontOrder = int.MinValue;
            float frontDistance = float.MaxValue;

            //SpawnPivot 직속 자식 -> 로드된 리소스 루트오브젝트
            foreach (Transform child in _objectPivot.Transform)
            {
                if (child.gameObject.activeInHierarchy == false)
                {
                    continue;
                }

                //캐릭터만 대상으로 설정
                if (child.TryGetComponent(out ICharacter character) == false)
                {
                    continue;
                }

                if (TryPickCharacter(child, character.ID, ray, out float distance, out int sortingOrder) == false)
                {
                    continue;
                }

                // 앞뒤 비교 값 확인용 추후 삭제 예정 @OH 26.11.04
                Debug.Log($"[SceneResourcePicker] 후보 ID={character.ID} | sortingOrder={sortingOrder} | distance={distance:F4}");

                if (IsInFront(sortingOrder, distance, frontOrder, frontDistance))
                {
                    frontId = character.ID;
                    frontOrder = sortingOrder;
                    frontDistance = distance;
                }
            }

            //임시 로그 @OH 26.11.04 삭제 예정
            Debug.Log($"[SceneResourePicker] {point} -> 결과 : {frontId ?? "없음"}");


            return (ResourceType.Character, frontId);
        }

        /// <summary>
        /// 캐릭터 하나를 Ray로 판정.
        /// 추후 판정 방식 변경해야하면 이 함수만 수정하면 됨
        /// 에셋번들에 CubismRaycaster 추가 후 CubismSDK에서 제공하는 함수들로 판정 추가하고 없을 때만 대체 판정을 하도록 했음 @OH 26.11.04
        /// </summary>
        /// <param name="root">캐릭터 루트(SpawnPivot 직속 자식)</param>
        /// <param name="id">경고 로그용 캐릭터 ID</param>
        /// <param name="ray">렌더 카메라 기준 월드 Ray</param>
        /// <param name="distance">Ray orgin - hit 거리</param>
        /// <param name="sortingOrder">앞 뒤 비교용 값</param>
        private bool TryPickCharacter(Transform root, string id, Ray ray, out float distance, out int sortingOrder)
        {
            distance = 0.0f;
            sortingOrder = 0;

            //판정과 앞뒤 비교에 같은 모델을 쓰므로 한 번만 찾음
            CubismModel model = root.GetComponentInChildren<CubismModel>();
            if(model == null)
            {
                return false;
            }

            //CubismRaycaster가 없을 경우 대체 판정
            if(TryGetModelBounds(model, out Bounds bounds) == false)
            {
                return false;
            }

            WarnIdOnce(id);
            
            if(bounds.IntersectRay(ray, out distance) == false)
            {
                return false;
            }

            sortingOrder = GetFrontSortingOrder(model);

            //레이 시각화
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 3f);
            return true;

        }

        /// <summary>
        /// 모델의 보이는 Drawable 범위를 모두 합친 사각형을 구함
        /// Cubism이 직접 관리하는 CubismRenderer.Mesh의 로컬 범위를 사용
        /// CubismSDK의 raycast 소스 방식을 참고하여 작성하였음
        /// </summary>
        private bool TryGetModelBounds(CubismModel model, out Bounds bounds)
        {
            bounds = default;

            bool hasBounds = false;
            CubismDrawable[] drawables = model.Drawables;

            for(int i = 0; i < drawables.Length; i ++)
            {
                if (drawables[i].TryGetComponent(out CubismRenderer renderer) == false
                || renderer.Mesh == null)
                {
                    continue;
                }

                // 꺼진 렌더러는 화면에 그려지지 않으므로 제외
                if(renderer.MeshRenderer != null
                 && renderer.MeshRenderer.enabled == false)
                {
                    continue;
                }

                EncapsulateLocalBounds(renderer.transform, renderer.Mesh.bounds, ref bounds, ref hasBounds);
            }

            if(hasBounds)
            {
                bounds.Expand(new Vector3(0.0f, 0.0f, _flatBoundsThickness));
            }

            return hasBounds;

        }

        /// <summary>
        /// 로컬 범위를 월드 좌표로 바꾼 후 범위를 누적산
        /// 꼭짓점 8개를 모두 변환해서 Drawable이 회전해 있어도 월드 범위가 정확하게 감싸지게 하였음
        /// </summary>
        /// <param name="space"></param>
        /// <param name="local"></param>
        /// <param name="world"></param>
        /// <param name="hasBounds"></param>
        private void EncapsulateLocalBounds(Transform space, Bounds local, ref Bounds world, ref bool hasBounds)
        {
            Vector3 min = local.min;
            Vector3 max = local.max;

            _corners[0] = new Vector3(min.x, min.y, min.z);
            _corners[1] = new Vector3(max.x, min.y, min.z);
            _corners[2] = new Vector3(min.x, max.y, min.z);
            _corners[3] = new Vector3(max.x, max.y, min.z);
            _corners[4] = new Vector3(min.x, min.y, max.z);
            _corners[5] = new Vector3(max.x, min.y, max.z);
            _corners[6] = new Vector3(min.x, max.y, max.z);
            _corners[7] = new Vector3(max.x, max.y, max.z);

            for (int c = 0; c < _corners.Length; c++)
            {
                Vector3 corner = space.TransformPoint(_corners[c]);

                if(hasBounds == false)
                {
                    //첫 점은 크기가 0인 범위로 시작
                    world = new Bounds(corner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    world.Encapsulate(corner);
                }
            }
        }

        private bool IsInFront(int order, float distance, int frontOrder, float frontDistance)
        {
            // 그리는 순서가 다르면 큰 쪽이 앞
            if(order != frontOrder)
            {
                return order > frontOrder;
            }

            //같으면 카메라에 가까운 쪽이 앞
            // 거리마저 같으면 나중에 순회한 쪽을 선택(SpawnPivot 계층에서 아래쪽)
            return distance <= frontDistance;
        }

        /// <summary>
        /// 모델의 보이는 drawable 중 가장 앞에 그려지는 렌더러의 sortingOrder
        /// 모델마다 Sorting Mode가 달라서 근사값으로 일단 구현부터 했음
        /// 나중에 캐릭터 모델 프리팹들 order 설정 표준화되면 수정해야할듯 @OH 26.11.04
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        private int GetFrontSortingOrder(CubismModel model)
        {
            int maxOrder = int.MinValue;
            CubismDrawable[] drawables = model.Drawables;

            for(int i = 0; i < drawables.Length; i++)
            {
                if (drawables[i].TryGetComponent(out CubismRenderer cubismRenderer) == false)
                {
                    continue;
                }

                MeshRenderer meshRenderer = cubismRenderer.MeshRenderer;
                if(meshRenderer == null || meshRenderer.enabled == false)
                {
                    continue;
                }

                if(meshRenderer.sortingOrder > maxOrder)
                {
                    maxOrder = meshRenderer.sortingOrder;
                }
            }

            //보이는 렌더러가 없으면 가장 뒤로 설정
            return maxOrder;
        }
        private void WarnIdOnce(string id)
        {
            if(_alreadyWarnedIds.Add(id))
            {
                Debug.LogWarning($"[SceneResourcePicker] 피킹 설정(CubismRaycaster)이 없어서 모델 범위로 대체 판정합니다. ID = {id}");
            }
        }

    }
}
