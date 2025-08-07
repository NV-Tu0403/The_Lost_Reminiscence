namespace echo17.EndlessBook.Demo02
{
    using System;
    using UnityEngine;

    /// <summary>
    /// class này đại diện cho một touch pad để tương tác với sách.
    /// </summary>
    public class TouchPad_02 : MonoBehaviour
    {
#if ENABLE_INPUT_SYSTEM
        /// <summary>
        /// New Input System actions mapping
        /// </summary>
        protected BookInputActions _bookInputActions;
#endif

        /// <summary>
        /// tên của các collider trang và mục lục.
        /// </summary>
        protected const string PageLeftColliderName = "Page Left";
        protected const string PageRightColliderName = "Page Right";
        protected const string TableOfContentsColliderName = "TableOfContents Button";

        /// <summary>
        /// giá trị ngưỡng kéo để phát hiện kéo trang.
        /// </summary>
        protected const float DragThreshold = 0.007f;

        /// <summary>
        /// kích thước của các trang.
        /// </summary>
        protected Rect[] pageRects;

        /// <summary>
        /// đại diện cho việc chạm xuống trang.
        /// </summary>
        protected bool touchDown;

        /// <summary>
        /// Vị trí chạm xuống trang.
        /// </summary>
        protected Vector2 touchDownPosition;

        /// <summary>
        /// Vị trí kéo trang cuối cùng.
        /// </summary>
        protected Vector2 lastDragPosition;

        /// <summary>
        /// Whether we are dragging
        /// </summary>
        protected bool dragging;

        /// <summary>
        /// enum đại diện cho các trang trong sách.
        /// </summary>
        public enum PageEnum
        {
            Left,
            Right
        }

        public Camera mainCamera;

        /// <summary>
        /// là mảng các collider của các trang.
        /// </summary>
        public Collider[] pageColliders;

        /// <summary>
        /// Collider của mục lục.
        /// </summary>
        public Collider tableOfContentsCollider;

        /// <summary>
        /// Layer mask để xác định các collider của trang và mục lục.
        /// </summary>
        public LayerMask pageTouchPadLayerMask;

        /// <summary>
        /// Biến này cho phép bạn gán một hàm xử lý sự kiện khi người dùng chạm xuống một trang sách. 
        /// Khi sự kiện xảy ra, hàm được gán sẽ nhận thông tin về trang bị chạm và vị trí chạm.
        /// </summary>
        public Action<PageEnum, Vector2> touchDownDetected;

        /// <summary>
        /// Handler for when a touch up is detected
        /// </summary>
        public Action<PageEnum, Vector2, bool> touchUpDetected;

        /// <summary>
        /// Handler for when a drag is detected
        /// </summary>
        public Action<PageEnum, Vector2, Vector2, Vector2> dragDetected;

        /// <summary>
        /// xử lý sự kiện khi người dùng chạm vào mục lục.
        /// </summary>
        public Action tableOfContentsDetected;

#if ENABLE_INPUT_SYSTEM
        private void OnEnable()
        {
            // set up the new input system actions
            _bookInputActions = new BookInputActions();
            _bookInputActions.TouchPad.Enable();
        }

        private void OnDisable()
        {
            // disable the new input system actions
            _bookInputActions.TouchPad.Disable();
        }
#endif

        void Awake()
        {
#if ENABLE_INPUT_SYSTEM
            Debug.Log("TouchPad: Using new Input System");
#endif

#if ENABLE_LEGACY_INPUT_MANAGER
            Debug.Log("TouchPad: Using old Input System");
#endif

            // cài đặt các giá trị mặc định

            pageRects = new Rect[2];
            for (var i = 0; i < 2; i++)
            {
                // lấy kích thước của các trang từ collider (quan trọng)
                pageRects[i] = new Rect(pageColliders[i].bounds.min.x, pageColliders[i].bounds.min.z, pageColliders[i].bounds.size.x, pageColliders[i].bounds.size.z);
            }
        }

        void Update()
        {
#if ENABLE_LEGACY_INPUT_MANAGER
            // Old Input System

            if (Input.GetMouseButtonDown(0))
            {
                // left mouse button pressed
                DetectTouchDown(Input.mousePosition);
            }
            if (Input.GetMouseButtonUp(0))
            {
                // left mouse button un-pressed
                DetectTouchUp(Input.mousePosition);
            }
            else if (touchDown && Input.GetMouseButton(0))
            {
                // dragging
                DetectDrag(Input.mousePosition);
            }
#endif

#if ENABLE_INPUT_SYSTEM
            // New Input System

            if (_bookInputActions.TouchPad.Press.WasPressedThisFrame())
            {
                // left mouse button pressed
                DetectTouchDown(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            }
            else if (_bookInputActions.TouchPad.Press.WasReleasedThisFrame())
            {
                // left mouse button un-pressed
                DetectTouchUp(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            }
            else if (touchDown && _bookInputActions.TouchPad.Press.IsPressed())
            {
                // dragging
                DetectDrag(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            }
#endif
        }

        /// <summary>
        /// bật hoặc tắt một trang collider.
        /// dùng khi cần bật hoặc tắt một trang cụ thể (kể cả collier),
        /// </summary>
        /// <param name="page">The page collider to toggle</param>
        /// <param name="on">Whether to toggle on</param>
        public virtual void Toggle(PageEnum page, bool on)
        {
            // activate or deactive the collider
            pageColliders[(int)page].gameObject.SetActive(on);
        }

        /// <summary>
        /// bật hoặc tắt mục lục.
        /// dùng khi đang lật trang và không muốn người dùng chạm vào mục lục,
        /// bật lại khi đã lật xong trang hoặc khi người dùng chạm vào mục lục.
        /// </summary>
        /// <param name="on">Whether to toggle on</param>
        public virtual void ToggleTableOfContents(bool on)
        {
            // bật hoặc tắt mục lục collider
            tableOfContentsCollider.gameObject.SetActive(on);
        }

        /// <summary>
        /// trả về thông tin chạm khi người dùng chạm vào trang hoặc mục lục.
        /// </summary>
        /// <param name="position"></param>
        protected virtual void DetectTouchDown(Vector2 position)
        {
            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // lấy điểm chạm từ vị trí chuột
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // nếu là mục lục, không cần xử lý trang
                touchDown = true;
                dragging = false;

                if (tableOfContents)
                {
                    // mục lục hit
                    tableOfContentsDetected();
                }
                else
                {
                    // trang hit
                    touchDownPosition = hitPosition;
                    lastDragPosition = hitPosition;

                    if (touchDownDetected != null)
                    {
                        // gọi handler cho touch down
                        touchDownDetected(page, hitPositionNormalized);
                    }
                }
            }
        }

        /// <summary>
        /// trả về thông tin chạm khi người dùng kéo tay
        /// </summary>
        /// <param name="position"></param>
        protected virtual void DetectDrag(Vector2 position)
        {
            // thoát nếu không có handler cho drag
            if (dragDetected == null) return;

            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // lấy điểm chạm từ vị trí chuột
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // nếu là mục lục, không cần xử lý trang
                var offset = hitPosition - lastDragPosition;

                // nếu là mục lục, không cần xử lý trang
                if (offset.magnitude >= DragThreshold)
                {
                    // đã kéo trang

                    dragging = true;
                    dragDetected(page, touchDownPosition, hitPosition, offset);
                    lastDragPosition = hitPosition;
                }
            }
        }

        /// <summary>
        /// trả về thông tin chạm khi người dùng nhả tay
        /// dùng để phát hiện khi người dùng chạm vào trang hoặc mục lục.
        /// </summary>
        /// <param name="position"></param>
        protected virtual void DetectTouchUp(Vector2 position)
        {
            // thoát nếu không có handler cho touch up
            if (touchUpDetected == null) return;

            Vector2 hitPosition;
            Vector2 hitPositionNormalized;
            PageEnum page;
            bool tableOfContents;

            // lấy điểm chạm từ vị trí chuột
            if (GetHitPoint(position, out hitPosition, out hitPositionNormalized, out page, out tableOfContents))
            {
                // nếu là mục lục, không cần xử lý trang
                touchDown = false;

                // nếu là mục lục, gọi handler cho mục lục
                touchUpDetected(page, hitPositionNormalized, dragging);
            }
        }

        /// <summary>
        /// heppel để lấy vị trí chạm từ vị trí chuột.
        ///  trả về vị trí chạm, vị trí chạm đã chuẩn hóa, trang và trạng thái của mục lục.
        /// </summary>
        /// <param name="mousePosition"></param>
        /// <param name="hitPosition"></param>
        /// <param name="hitPositionNormalized"></param>
        /// <param name="page"></param>
        /// <param name="tableOfContents"></param>
        /// <returns></returns>
        protected virtual bool GetHitPoint(Vector3 mousePosition, out Vector2 hitPosition, out Vector2 hitPositionNormalized, out PageEnum page, out bool tableOfContents)
        {
            hitPosition = Vector2.zero;
            hitPositionNormalized = Vector2.zero;
            page = PageEnum.Left;
            tableOfContents = false;

            // tạo ray từ vị trí chuột
            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            // kiểm tra xem ray có va chạm với các collider của trang không
            if (Physics.Raycast(ray, out hit, 1000, pageTouchPadLayerMask))
            {
                // xác định trang dựa trên tên collider
                page = hit.collider.gameObject.name == PageLeftColliderName ? PageEnum.Left : PageEnum.Right;

                // kiểm tra xem va chạm có phải là mục lục không
                tableOfContents = hit.collider.gameObject.name == TableOfContentsColliderName;

                // nếu là mục lục, không cần tính toán hitPosition và hitPositionNormalized
                var pageIndex = (int)page;

                // lấy vị trí va chạm
                hitPosition = new Vector2(hit.point.x, hit.point.z);

                // tính toán vị trí đã chuẩn hóa dựa trên kích thước của trang
                hitPositionNormalized = new Vector2((hit.point.x - pageRects[pageIndex].xMin) / pageRects[pageIndex].width,
                                                        (hit.point.z - pageRects[pageIndex].yMin) / pageRects[pageIndex].height
                                                        );

                return true; // đã chạm vào một trang hoặc mục lục
            }

            return false;
        }
    }
}