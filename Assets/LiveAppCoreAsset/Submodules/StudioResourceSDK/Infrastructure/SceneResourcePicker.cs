using Live2D.Cubism.Core;
using Live2D.Cubism.Framework.Raycasting;
using Live2D.Cubism.Rendering;
using LiveAppCore;
using StudioCharacterSDK.Domain;
using StudioCommonSDK.Domain;
using StudioResourceSDK.Domain;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾고 리소스의 화면 범위를 구할 infra
    /// 현재는 캐릭터만 지원 @OH 26.11.04
    /// </summary>
    public class SceneResourcePicker : IResourcePickDomain
    {
        private readonly ISpawnPivotTransform _objectPivot;
        /// <summary>
        /// ray hit 판별용 보정값
        /// </summary>
        private const float _flatBoundsThickness = 0.001f;
        /// <summary>
        /// 해당 값 이하의 투명도는 화면에 보이지 않는 걸로 판정
        /// </summary>
        private const float _invisibleOpacity = 0.01f;
        /// <summary>
        /// 로컬 범위의 꼭짓점을 저장. 카메라가 perspective라서 8개 설정
        /// </summary>
        private static readonly Vector3[] _corners = new Vector3[8];
        /// <summary>
        /// 정점 색상 read용 버퍼
        /// </summary>
        private static readonly List<Color> _vertexColorBuffer = new List<Color>();
        /// <summary>
        /// SDK Raycast 결과 버퍼 - CubismSDK의 Raycaster 사용하는 모델용
        /// </summary>
        private readonly CubismRaycastHit[] _raycastHits = new CubismRaycastHit[4];
       /// <summary>
        /// 대체 판정 경고를 이미 출력한 캐릭터 id. 클릭할 때마다 같은 경고가 쌓이지 않도록 하기 위함
        /// </summary>
        private readonly HashSet<string> _alreadyWarnedIds = new HashSet<string>();

        public SceneResourcePicker([Inject( Id = SpawnPivotId.Object)] ISpawnPivotTransform objectPivot)
        {
            _objectPivot = objectPivot;
        }


        // ----------------------------------------------------------------------------implement
        /// <summary>
        /// 피킹 메인 프로세스
        /// </summary>
        /// <param name="point">play 화면 상에서 클릭한 지점</param>
        /// <returns>피킹된 리소스의 튜플 -nullable</returns>
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

            //카메라와 가장 가까운 오브젝트 기준
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

                //캐릭터만 대상으로 설정    @OH 26.11.06
                if (child.TryGetComponent(out ICharacter character) == false)
                {
                    continue;
                }

                if (TryPickCharacter(child, character.ID, ray, renderCamera.farClipPlane, 
                    out float distance, out int sortingOrder) == false)
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

            if(frontId == null)
            {   
                return null;
            }

            return (ResourceType.Character, frontId);
        }

        /// <summary>
        /// 선택 표시 테두리UI는 모델 전체 범위로 그림 -> 어떤 캐릭터를 선택했는 지를 직관적으로 보여주기 위함
        /// Raycaster가 있는 모델은 피킹 판정 영역(HitArea)이 테두리보다 작을 수 있음 @OH 26.11.06 
        /// </summary>
        /// <param name="type">리소스 타입</param>
        /// <param name="id">리소스 오브젝트 id</param>
        /// <param name="rect">판정 Rect</param>
        public bool TryGetViewportRect(ResourceType type, string id, out ViewportRect rect)
        {
            rect = default;

            //현재는 캐릭터만 지원 @OH 26.11.05
            if( type != ResourceType.Character)
            {
                return false;
            }

            Camera renderCamera = Camera.main;
            if(renderCamera == null)
            {
                return false;
            }

            if(TryFindCharacterModel(id, out CubismModel model) == false)
            {
                return false;
            }

            if (TryGetModelBounds(model, null, out Bounds worldBounds) == false)
            {
                return false;
            }

            FillCorners(worldBounds);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;

            for(int c = 0; c < _corners.Length; c++)
            {
                Vector3 viewport = renderCamera.WorldToViewportPoint(_corners[c]);

                if (viewport.x < minX) minX = viewport.x;
                if (viewport.y < minY) minY = viewport.y;
                if (viewport.x > maxX) maxX = viewport.x;
                if (viewport.y > maxY) maxY = viewport.y;
            }

            rect = new ViewportRect(minX, minY, maxX, maxY);
            return true;
        }
        // -------------------------------------------------------------------------/implement

        /// <summary>
        /// 캐릭터 하나를 Ray로 판정.
        /// 추후 판정 방식 변경해야하면 이 함수만 수정하면 됨
        /// 에셋번들에 CubismRaycaster 추가 후 CubismSDK에서 제공하는 함수들로 판정 추가하고 없을 때만 대체 판정을 하도록 했음 @OH 26.11.04
        /// </summary>
        /// <param name="root">캐릭터 루트(SpawnPivot 직속 자식)</param>
        /// <param name="id">경고 로그용 캐릭터 ID</param>
        /// <param name="ray">렌더 카메라 기준 월드 Ray</param>
        /// <param name="distance">Ray탐지할 최대 거리</param>
        /// <param name="distance">Ray orgin - hit 거리</param>
        /// <param name="sortingOrder">앞 뒤 비교용 값</param>
        private bool TryPickCharacter(Transform root, string id, Ray ray, float maxDistance, out float distance, out int sortingOrder)
        {
            distance = 0.0f;
            sortingOrder = 0;

            //레이 시각화
            Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 3f);

            //판정과 앞뒤 비교에 같은 모델을 쓰므로 한 번만 찾음
            CubismModel model = root.GetComponentInChildren<CubismModel>();
            if(model == null)
            {
                return false;
            }

            bool isHit = model.TryGetComponent(out CubismRaycaster raycaster)
                ? TryPickByRaycaster(raycaster, ray, maxDistance, out distance)
                : TryPickByModelBounds(model, id, ray, out distance);

            if(isHit == false)
            {
                return false;
            }

            sortingOrder = GetFrontSortingOrder(model);
            return true;

        }

        /// <summary>
        /// CubismSDK의 Raycaster로 hit판정하며 Raycastable이 붙은 메쉬만 검사함
        /// </summary>
        /// <param name="raycaster"></param>
        /// <param name="ray"></param>
        /// <param name="maxDistance"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private bool TryPickByRaycaster(CubismRaycaster raycaster, Ray ray, float maxDistance, out float distance)
        {
            distance = 0.0f;
            int hitCount;

            hitCount = raycaster.Raycast(ray, _raycastHits, maxDistance);

            if(hitCount == 0)
            {
                return false;
            }

            //맞은 메쉬 중 가장 가까운 거리
            distance = float.MaxValue;
            for(int i = 0; i < hitCount; i++)
            {
                if (_raycastHits[i].Distance < distance)
                {
                    distance = _raycastHits[i].Distance;
                }
            }

            return true;
        }

        /// <summary>
        /// CubismModel이 Raycaster가 없을 경우 모델의 월드 기준 전체 범위로 ray hit 판정
        /// 현재 default 방식으로 결정 @OH 26.11.06
        /// </summary>
        /// <param name="model">캐릭터 모델</param>
        /// <param name="id">해당 모델 id</param>
        /// <param name="ray"></param>
        /// <param name="distance">ray - hit 거리</param>
        /// <returns></returns>
        private bool TryPickByModelBounds(CubismModel model, string id, Ray ray, out float distance)
        {
            distance = 0.0f;

            if (TryGetModelBounds(model, null, out Bounds bounds) == false)
            {
                return false;
            }

            //평면 모델은 z값이 0 이니 ray 교차 판정이 실패하지 않도록 최소 두께를 설정
            bounds.Expand(new Vector3(0.0f, 0.0f, _flatBoundsThickness));

            WarnIdOnce(id);

            return bounds.IntersectRay(ray, out distance);
        }

        /// <summary>
        /// 모델의 보이는 Drawable 범위를 모두 합친 사각형을 구함
        /// </summary>
        /// <param name="model">대상 모델</param>
        /// <param name="space">결과 bound범위의 기준 공간. null ->월드 좌표 (피킹용), 모델 Transform -> 모델 로컬 좌표 (선택 UI표시용)</param>
        /// <param name="bounds">기준 공간에서의 bound범위</param> 
        /// Cubism이 직접 관리하는 CubismRenderer.Mesh의 로컬 범위를 사용
        /// CubismSDK의 raycast 소스 방식을 참고하여 작성하였음
        private bool TryGetModelBounds(CubismModel model,Transform space ,out Bounds bounds)
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

                // 화면에 보이지 않는 메쉬는 제외
                if(IsVisibleDrawable(renderer) == false)
                {
                    continue;
                }

                EncapsulateLocalBounds(renderer.transform, space,renderer.Mesh.bounds, ref bounds, ref hasBounds);
            }

            return hasBounds;

        }

        /// <summary>
        /// 로컬 범위를 월드 좌표로 바꾼 후 범위를 누적산
        /// </summary>
        /// <param name="from">범위가 속한 공간(drawable의 tranform)</param>
        /// <param name="space">결과 기준 공간. null -> 월드 기준</param>
        private void EncapsulateLocalBounds(Transform from, Transform space, Bounds local, ref Bounds result, ref bool hasBounds)
        {
            FillCorners(local);

            for (int c = 0; c < _corners.Length; c++)
            {
                //drawable 로컬 -> 월드
                Vector3 corner = from.TransformPoint(_corners[c]);

                //기준 공간이 있으면 월드 -> 기준 공간 로컬
                if(space != null)
                {
                    corner = space.InverseTransformPoint(corner);
                }

                if(hasBounds == false)
                {
                    //첫 점은 크기가 0인 범위로 시작
                    result = new Bounds(corner, Vector3.zero);
                    hasBounds = true;
                }
                else
                {
                    result.Encapsulate(corner);
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
        /// </summary>
        /// <returns>결과 sorting order</returns>
        /// 모델마다 Sorting Mode가 달라서 drawable 순회하며 sortingOrder를 직접 계산
        /// 나중에 캐릭터 모델 프리팹들 order 설정 표준화되면 
        /// RenderController.SortingOrder만 읽도록 수정해야할듯 @OH 26.11.04
        private int GetFrontSortingOrder(CubismModel model)
        {
            int maxOrder = int.MinValue;
            CubismDrawable[] drawables = model.Drawables;

            for(int i = 0; i < drawables.Length; i++)
            {
                if (drawables[i].TryGetComponent(out CubismRenderer cubismRenderer) == false
                || IsVisibleDrawable(cubismRenderer) == false ) 
                {
                    continue;
                }

                int order = cubismRenderer.MeshRenderer.sortingOrder;

                if(order > maxOrder)
                {
                    maxOrder = order;
                }
            }

            //보이는 렌더러가 없으면 가장 뒤로 설정
            return maxOrder;
        }

        /// <summary>
        /// SpawnPivot의 직속 자식 중 id가 같은 캐릭터의 모델을 찾음
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private bool TryFindCharacterModel(string id, out CubismModel model)
        {
            model = null;

            if(_objectPivot == null 
             ||_objectPivot.Transform == null)
            {
                return false;
            }

            foreach(Transform child in _objectPivot.Transform)
            {
                if(child.TryGetComponent(out ICharacter character)
                    && character.ID == id)
                {
                    model = child.GetComponentInChildren<CubismModel>();
                    return model != null;
                }
            }

            return false;
        }
        
        /// <summary>
        /// _corner를 bounds의 꼭짓점으로 채움
        /// </summary>
        private void FillCorners(Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            _corners[0] = new Vector3(min.x, min.y, min.z);
            _corners[1] = new Vector3(max.x, min.y, min.z);
            _corners[2] = new Vector3(min.x, max.y, min.z);
            _corners[3] = new Vector3(max.x, max.y, min.z);
            _corners[4] = new Vector3(min.x, min.y, max.z);
            _corners[5] = new Vector3(max.x, min.y, max.z);
            _corners[6] = new Vector3(min.x, max.y, max.z);
            _corners[7] = new Vector3(max.x, max.y, max.z);
        }

        /// <summary>
        /// 화면에 실제로 보이는 drawable Mesh인지 판단
        /// </summary>
        private bool IsVisibleDrawable(CubismRenderer renderer)
        {
            //렌더러가 꺼져 있으면 보이지 않는 것으로 판단
            if (renderer.MeshRenderer != null 
             && renderer.MeshRenderer.enabled == false)
            {
                return false;
            }

            Mesh mesh = renderer.Mesh;
            if (mesh == null)
            {
                return false;
            }

            // CubismRenderer.Opacity가 internal이라 접근할 수 없지만 
            // sdk에서 해당 값을 통해 정점 색상 알파에 계산하므로 해당 값을 사용하는 방식으로 결정
            mesh.GetColors(_vertexColorBuffer);

            // 정점 색상이 없으면 판단할 수 없으므로 보이는 것으로 취급
            if (_vertexColorBuffer.Count == 0)
            {
                return true;
            }

            // SDK가 모든 정점에 같은 색을 넣으므로 첫 정점만 확인
            return _vertexColorBuffer[0].a > _invisibleOpacity;
        }

        private void WarnIdOnce(string id)
        {
            if(_alreadyWarnedIds.Add(id))
            {
                Debug.LogWarning($"[SceneResourcePicker] CubismRaycaster이 없는 모델. AABB박스 방식으로 피킹합니다.  ID = {id}");
            }
        }

    }
}
