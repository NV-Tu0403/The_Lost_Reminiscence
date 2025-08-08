namespace echo17.EndlessBook.Demo02
{
    using System.Linq;
    using UnityEngine;
    using echo17.EndlessBook;
    using System.Threading.Tasks;

    public enum BookActionTypeEnum
    {
        ChangeState,
        TurnPage
    }

    public delegate void BookActionDelegate(BookActionTypeEnum actionType, int actionValue);

    public class Demo02 : MonoBehaviour
    {
        protected bool audioOn = false;
        protected bool flipping = false;
        public EndlessBook book;
        public float openCloseTime = 0.3f;
        public EndlessBook.PageTurnTimeTypeEnum groupPageTurnType;
        public float singlePageTurnTime;
        public float groupPageTurnTime;
        public int tableOfContentsPageNumber;
        public AudioSource bookOpenSound;
        public AudioSource bookCloseSound;
        public AudioSource pageTurnSound;
        public AudioSource pagesFlippingSound;
        public float pagesFlippingSoundDelay;
        public TouchPad touchPad;
        public UIPageView[] uiPageViews;
        public CameraZoomController cameraZoomController;

        private TaskCompletionSource<bool> turnPageTcs;

        void Start()
        {
            touchPad.touchDownDetected = TouchPadTouchDownDetected;
            touchPad.touchUpDetected = TouchPadTouchUpDetected;
            touchPad.tableOfContentsDetected = TableOfContentsDetected;
            touchPad.dragDetected = TouchPadDragDetected;

            // khởi tạo các UIPageView
            foreach (var uiPageView in uiPageViews)
            {
                if (uiPageView != null)
                {
                    uiPageView.onTouchDownDetected = UIPageViewTouchDownDetected;
                    uiPageView.onTouchUpDetected = UIPageViewTouchUpDetected;
                    uiPageView.onHoverDetected = UIPageViewHoverDetected;
                }
            }

            OnBookStateChanged(EndlessBook.StateEnum.ClosedFront, EndlessBook.StateEnum.ClosedFront, -1);
            audioOn = true;
        }

        protected virtual void OnBookStateChanged(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int pageNumber)
        {
            switch (toState)
            {
                case EndlessBook.StateEnum.ClosedFront:
                case EndlessBook.StateEnum.ClosedBack:
                    if (audioOn)
                    {
                        bookCloseSound.Play();
                    }
                    TurnOffAllUIPageViews();
                    break;

                case EndlessBook.StateEnum.OpenMiddle:
                    if (fromState != EndlessBook.StateEnum.OpenMiddle)
                    {
                        bookOpenSound.Play();
                    }
                    else
                    {
                        flipping = false;
                        pagesFlippingSound.Stop();
                    }
                    ToggleUIPageView(0, false);
                    ToggleUIPageView(999, false);
                    ToggleUIPageView(book.CurrentLeftPageNumber, true);
                    ToggleUIPageView(book.CurrentRightPageNumber, true);
                    break;

                case EndlessBook.StateEnum.OpenFront:
                case EndlessBook.StateEnum.OpenBack:
                    bookOpenSound.Play();
                    ToggleUIPageView(toState == EndlessBook.StateEnum.OpenFront ? 0 : 999, true);
                    break;
            }

            ToggleTouchPad(toState != EndlessBook.StateEnum.OpenMiddle); // Tắt TouchPad khi ở OpenMiddle
        }

        protected virtual void ToggleTouchPad(bool on)
        {
            // Bật/tắt toàn bộ GameObject của TouchPad
            if (touchPad != null)
            {
                touchPad.gameObject.SetActive(on);
                if (debugMode)
                {
                    //Debug.Log($"[Demo02] TouchPad GameObject {(on ? "enabled" : "disabled")} in state: {book.CurrentState}");
                }
            }
        }

        protected virtual void TurnOffAllUIPageViews()
        {
            for (var i = 0; i < uiPageViews.Length; i++)
            {
                if (uiPageViews[i] != null)
                {
                    uiPageViews[i].Deactivate();
                }
            }
        }

        protected virtual void ToggleUIPageView(int pageNumber, bool on)
        {
            var uiPageView = GetUIPageView(pageNumber);
            if (uiPageView != null)
            {
                if (on)
                {
                    uiPageView.Activate();
                }
                else
                {
                    uiPageView.Deactivate();
                }
            }
        }

        protected virtual void OnPageTurnStart(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            if (!flipping)
            {
                pageTurnSound.Play();
            }

            ToggleTouchPad(false);

            ToggleUIPageView(pageNumberFront, true);
            ToggleUIPageView(pageNumberBack, true);

            switch (turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:
                    ToggleUIPageView(pageNumberLastVisible, true);
                    break;

                case Page.TurnDirectionEnum.TurnBackward:
                    ToggleUIPageView(pageNumberFirstVisible, true);
                    break;
            }
        }

        protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            switch (turnDirection)
            {
                case Page.TurnDirectionEnum.TurnForward:
                    ToggleUIPageView(pageNumberFirstVisible - 1, false);
                    ToggleUIPageView(pageNumberFirstVisible - 2, false);
                    break;

                case Page.TurnDirectionEnum.TurnBackward:
                    ToggleUIPageView(pageNumberLastVisible + 1, false);
                    ToggleUIPageView(pageNumberLastVisible + 2, false);
                    break;

            }

            turnPageTcs?.TrySetResult(true); // Báo hiệu đã xong
        }

        protected virtual async void TableOfContentsDetected()
        {
            await TurnToPage(tableOfContentsPageNumber, true);
        }

        protected virtual void TouchPadTouchDownDetected(TouchPad.PageEnum page, Vector2 hitPointNormalized)
        {
            // Xử lý touch down từ TouchPad
        }

        protected virtual void TouchPadTouchUpDetected(TouchPad.PageEnum page, Vector2 hitPointNormalized, bool dragging)
        {
            switch (book.CurrentState)
            {
                case EndlessBook.StateEnum.ClosedFront:
                    if (page == TouchPad.PageEnum.Right)
                    {
                        OpenFront();
                    }
                    break;

                case EndlessBook.StateEnum.OpenFront:
                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:
                            ClosedFront();
                            break;
                        case TouchPad.PageEnum.Right:
                            OpenMiddle();
                            break;
                    }
                    break;

                case EndlessBook.StateEnum.OpenMiddle:
                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:
                            if (book.CurrentLeftPageNumber == 1)
                            {
                                OpenFront();
                            }
                            else
                            {
                                book.TurnBackward(singlePageTurnTime, onCompleted: OnBookStateChanged, onPageTurnStart: OnPageTurnStart, onPageTurnEnd: OnPageTurnEnd);
                            }
                            break;

                        case TouchPad.PageEnum.Right:
                            if (book.CurrentRightPageNumber == book.LastPageNumber)
                            {
                                OpenBack();
                            }
                            else
                            {
                                book.TurnForward(singlePageTurnTime, onCompleted: OnBookStateChanged, onPageTurnStart: OnPageTurnStart, onPageTurnEnd: OnPageTurnEnd);
                            }
                            break;
                    }
                    break;

                case EndlessBook.StateEnum.OpenBack:
                    switch (page)
                    {
                        case TouchPad.PageEnum.Left:
                            OpenMiddle();
                            break;
                        case TouchPad.PageEnum.Right:
                            ClosedBack();
                            break;
                    }
                    break;

                case EndlessBook.StateEnum.ClosedBack:
                    if (page == TouchPad.PageEnum.Left)
                    {
                        OpenBack();
                    }
                    break;
            }
        }

        protected virtual void TouchPadDragDetected(TouchPad.PageEnum page, Vector2 touchDownPosition, Vector2 currentPosition, Vector2 incrementalChange)
        {
            // Xử lý drag từ TouchPad
        }

        /// <summary>
        /// Xử lý sự kiện TouchDown trên UIPageView.
        /// gọi đến các tác vụ nhiệp vụ như new game, lưu game, v.v.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="item"></param>
        protected virtual void UIPageViewTouchDownDetected(Vector2 screenPoint, UIItem item)
        {
            if (debugMode)
            {
                // Debug.Log($"[Demo02] UIPageView TouchDown on {item.uIActionType}");
            }

        }

        protected virtual void UIPageViewTouchUpDetected(Vector2 screenPoint, UIItem item, bool dragging)
        {
            if (debugMode)
            {
                Debug.Log($"[Demo02] TOUCH UP {item.uIActionType} - {item.targetRenderer.gameObject.name}.");
            }

            DeTectedMainMenu(item);
            DeTectedSave(item);
            DetectedAccoutFuntion(item);
            DetectedPauseSession(item);
        }

        /// <summary>
        /// Xử lý các hành động từ menu chính như New Game, Continue, SavePanel, Quit.
        /// </summary>
        /// <param name="item"></param>
        private async void DeTectedMainMenu(UIItem item)
        {
            switch (item.uIActionType)
            {
                case UIActionType.NewSession:
                    await TurnToPage(item.targetPage, false);
                    Core.Instance.ActiveMenu(true, false);
                    await cameraZoomController.PerformZoomSequence(0, () => CoreEvent.Instance.triggerNewSession(), true);
                    //CoreEvent.Instance.triggerNewSession();
                    await TurnToPage(7, false);
                    Core.Instance.ActiveMenu(false, false);
                    break;
                case UIActionType.SavePanel:
                    await TurnToPage(item.targetPage, true);
                    CoreEvent.Instance.triggerSavePanel();
                    break;
                case UIActionType.TutorialSession:
                    await TurnToPage(item.targetPage, false);
                    CoreEvent.Instance.triggerNewSession();
                    break;

                case UIActionType.QuitGame:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
            }
        }

        /// <summary>
        /// Xử lý các hành động từ menu lưu game như Select, Delete, Duplicate.
        /// </summary>
        /// <param name="item"></param>
        private async void DeTectedSave(UIItem item)
        {
            //Debug.LogWarning($"[Demo02] DeTectedSave: {item.uIActionType} - {item.targetRenderer.gameObject.name}");
            switch (item.uIActionType)
            {
                case UIActionType.Back:
                    await TurnToPage(item.targetPage, true);
                    break;
                case UIActionType.ContinueSession:
                    if (!string.IsNullOrWhiteSpace(ProfessionalSkilMenu.Instance.selectedSaveFolder))// Iem tà đạo (CHƯA CÓ TG FIX)
                    {
                        Core.Instance.ActiveMenu(true, false);
                        await cameraZoomController.PerformZoomSequence(0, () => CoreEvent.Instance.triggerContinueSession(), true);
                        Core.Instance.ActiveMenu(false, false);
                        if (!cameraZoomController.ZoomState)
                        {
                            await TurnToPage(item.targetPage, false);
                        }
                    }
                    break;
                case UIActionType.RefreshSaveList:
                    await UIPage05.Instance.RefreshSaveSlots();
                    break;
                case UIActionType.SelectSaveItem:
                    await UIPage05.Instance.GetFolderPathBySlotName(item.targetRenderer.gameObject.name, UIActionType.SelectSaveItem);
                    break;
                case UIActionType.DeleteSaveItem:
                    await UIPage05.Instance.GetFolderPathBySlotName(item.targetRenderer.gameObject.name, UIActionType.DeleteSaveItem);
                    break;
                case UIActionType.SyncFileSave:
                    CoreEvent.Instance.triggerSyncFileSave(ProfessionalSkilMenu.Instance.selectedSaveFolder);
                  
                    break;
            }
        }

        private bool isLogin = default;   //(cách này hơi tà đạo nhưng giờ iem làm biếng quá)
        private void DetectedAccoutFuntion(UIItem item)
        {
            string accountState = Core.Instance.CurrentAccountState;
            switch (item.uIActionType)
            {
                case UIActionType.Confim: // thực hiện Login/ Register/ ConnectToServer tùy AccountStateType
                    if (accountState == AccountStateType.NoCurrentAccount.ToString())
                    {
                        if (isLogin) CoreEvent.Instance.triggerLogin(); // đăng nhập
                        else CoreEvent.Instance.triggerRegister();      // đăng ký
                        UiPage06_C.Instance.ActiveObj(true, false, false, false);
                    }
                    isLogin = false;

                    if (accountState == AccountStateType.NoConnectToServer.ToString())
                    {
                        CoreEvent.Instance.triggerConnectToServer();    // kết nối đến server
                        UiPage06_C.Instance.ActiveObj(true, true, false, false);
                    }

                    if (accountState == AccountStateType.ConectingServer.ToString())
                    {
                        CoreEvent.Instance.triggerConnectingToServer();
                        UiPage06_C.Instance.ActiveObj(true, false, false, false);
                    }
         
                    break;

                case UIActionType.Cancel:           // xóa data input và tắt panel Input
                    UiPage06_C.Instance.ActiveObj(true, false, false, false);
                    break;

                case UIActionType.Logout:           // bật panel Input, tắt bt này tùy vào AccountStateType
                    if (accountState == AccountStateType.NoConnectToServer.ToString() || accountState == AccountStateType.HaveConnectToServer.ToString())
                    {
                        CoreEvent.Instance.triggerLogout(); // logout
                        UiPage06_C.Instance.ActiveObj(true, false, false, false);
                    }

                    if (accountState == AccountStateType.NoCurrentAccount.ToString())
                    {
                        isLogin = !isLogin; // đánh dấu là login
                        UiPage06_C.Instance.ActiveObj(false, true, false, true);
                    }

                    break;
                case UIActionType.ConnectToServer:  // bật panel Input, tắt bt này tùy vào AccountStateType
                    if (accountState == AccountStateType.NoConnectToServer.ToString()) // connect to server
                    {
                        UiPage06_C.Instance.ActiveObj(false, true, true, true);
                    }
                    if (accountState == AccountStateType.NoCurrentAccount.ToString()) // register
                    {
                        UiPage06_C.Instance.ActiveObj(false, true, false, true);
                    }
                    if (accountState == AccountStateType .HaveConnectToServer.ToString()) // nếu 
                    {
                        CoreEvent.Instance.triggerOverriceSave(); // ghi đè dữ liệu save
                        //UiPage06_C.Instance.ActiveObj(true, false, false, false);
                    }
                    break;
            }
        }

        private async void DetectedPauseSession(UIItem item)
        {
            switch (item.uIActionType)
            {
                case UIActionType.ResumeSession:
                    CoreEvent.Instance.triggerResumedSession();
                    break;
                case UIActionType.QuitSesion:
                    await TurnToPage(item.targetPage, true);
                    await cameraZoomController.PerformZoomSequence(0, () => CoreEvent.Instance.triggerQuitSession(), false);
                    break;
            }
        }

        protected virtual void UIPageViewHoverDetected(Vector2 screenPoint)
        {
            // Không cần xử lý hover ở đây vì UIPageView tự xử lý highlight
        }

        protected virtual UIPageView GetUIPageView(int pageNumber)
        {
            return uiPageViews.Where(x => x.name == string.Format("UIPageView_{0}", (pageNumber == 0 ? "Front" : (pageNumber == 999 ? "Back" : pageNumber.ToString("00"))))).FirstOrDefault();
        }

        protected virtual void ClosedFront()
        {
            SetState(EndlessBook.StateEnum.ClosedFront);
        }

        protected virtual void OpenFront()
        {
            ToggleUIPageView(0, true);
            SetState(EndlessBook.StateEnum.OpenFront);
        }

        public virtual void OpenMiddle()
        {
            ToggleUIPageView(book.CurrentLeftPageNumber, true);
            ToggleUIPageView(book.CurrentRightPageNumber, true);
            SetState(EndlessBook.StateEnum.OpenMiddle);
        }

        protected virtual void OpenBack()
        {
            ToggleUIPageView(999, true);
            SetState(EndlessBook.StateEnum.OpenBack);
        }

        protected virtual void ClosedBack()
        {
            SetState(EndlessBook.StateEnum.ClosedBack);
        }

        protected virtual void SetState(EndlessBook.StateEnum state)
        {
            ToggleTouchPad(state != EndlessBook.StateEnum.OpenMiddle); // Tắt TouchPad khi chuyển sang OpenMiddle
            book.SetState(state, openCloseTime, OnBookStateChanged);
        }

        protected virtual async Task TurnToPage(int pageNumber, bool isWait = true)
        {
            if (isWait) // Nếu cần đợi hoàn thành
                turnPageTcs = new TaskCompletionSource<bool>();

            var newLeftPageNumber = pageNumber % 2 == 0 ? pageNumber - 1 : pageNumber;

            if (Mathf.Abs(newLeftPageNumber - book.CurrentLeftPageNumber) > 2)
            {
                flipping = true;
                pagesFlippingSound.PlayDelayed(pagesFlippingSoundDelay);
            }

            book.TurnToPage(pageNumber, groupPageTurnType, groupPageTurnTime,
                openTime: openCloseTime,
                onCompleted: OnBookStateChanged,
                onPageTurnStart: OnPageTurnStart,
                onPageTurnEnd: OnPageTurnEnd); // << Hook ở đây

            if (isWait)
                await turnPageTcs.Task; // Chờ cho đến khi trang lật xong
        }

        // Callback khi trang lật xong
        //private void OnPageTurnEnd()
        //{
        //    // Thực hiện các xử lý nội bộ nếu cần
        //    turnPageTcs?.TrySetResult(true); // Báo hiệu đã xong
        //}

        public bool debugMode = false; // Thêm để debug
    }
}