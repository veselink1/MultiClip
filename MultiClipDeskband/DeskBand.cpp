#include <windows.h>
#include <uxtheme.h>
#include <gdiplus.h>
#include "DeskBand.h"
using namespace Gdiplus;

#define RECTWIDTH(x)   ((x).right - (x).left)
#define RECTHEIGHT(x)  ((x).bottom - (x).top)

extern ULONG        g_cDllRef;
extern HINSTANCE    g_hInst;

extern CLSID CLSID_DeskBand;

static const WCHAR g_szDeskBandClass[] = L"MultiClipDeskbandClass";
CDeskBand* CDeskBand::s_inst = NULL;

CDeskBand::CDeskBand()
	: m_cRef(1)
	, m_pSite(NULL)
	, m_fHasFocus(FALSE)
	, m_fIsDirty(FALSE)
	, m_dwBandID(0)
	, m_hwnd(NULL)
	, m_hwndParent(NULL)
	, m_gdipToken(NULL)
	, m_hMouseHook(NULL)
	, m_isMouseOver(false)
	, m_isMouseDown(false)
{
	GdiplusStartupInput gdipStartupInput;
	GdiplusStartup(&m_gdipToken, &gdipStartupInput, NULL);
	SetWindowsHookEx(WH_MOUSE_LL, MouseHookProc, g_hInst, 0);

	s_inst = this;
}

CDeskBand::~CDeskBand()
{
	UnhookWindowsHookEx(m_hMouseHook);
	if (m_pSite)
	{
		m_pSite->Release();
	}
	GdiplusShutdown(m_gdipToken);
	s_inst = NULL;
}

//
// IUnknown
//
STDMETHODIMP CDeskBand::QueryInterface(REFIID riid, void **ppv)
{
    HRESULT hr = S_OK;

    if (IsEqualIID(IID_IUnknown, riid)       ||
        IsEqualIID(IID_IOleWindow, riid)     ||
        IsEqualIID(IID_IDockingWindow, riid) ||
        IsEqualIID(IID_IDeskBand, riid)      ||
        IsEqualIID(IID_IDeskBand2, riid))
    {
        *ppv = static_cast<IOleWindow *>(this);
    }
    else if (IsEqualIID(IID_IPersist, riid) ||
             IsEqualIID(IID_IPersistStream, riid))
    {
        *ppv = static_cast<IPersist *>(this);
    }
    else if (IsEqualIID(IID_IObjectWithSite, riid))
    {
        *ppv = static_cast<IObjectWithSite *>(this);
    }
    else if (IsEqualIID(IID_IInputObject, riid))
    {
        *ppv = static_cast<IInputObject *>(this);
    }
    else
    {
        hr = E_NOINTERFACE;
        *ppv = NULL;
    }

    if (*ppv)
    {
        AddRef();
    }

    return hr;
}

STDMETHODIMP_(ULONG) CDeskBand::AddRef()
{
    return InterlockedIncrement(&m_cRef);
}

STDMETHODIMP_(ULONG) CDeskBand::Release()
{
    ULONG cRef = InterlockedDecrement(&m_cRef);
    if (0 == cRef)
    {
        delete this;
    }

    return cRef;
}

//
// IOleWindow
//
STDMETHODIMP CDeskBand::GetWindow(HWND *phwnd)
{
    *phwnd = m_hwnd;
    return S_OK;
}

STDMETHODIMP CDeskBand::ContextSensitiveHelp(BOOL)
{
    return E_NOTIMPL;
}

//
// IDockingWindow
//
STDMETHODIMP CDeskBand::ShowDW(BOOL fShow)
{
    if (m_hwnd)
    {
        ShowWindow(m_hwnd, fShow ? SW_SHOW : SW_HIDE);
    }

    return S_OK;
}

STDMETHODIMP CDeskBand::CloseDW(DWORD)
{
    if (m_hwnd)
    {
        ShowWindow(m_hwnd, SW_HIDE);
        DestroyWindow(m_hwnd);
        m_hwnd = NULL;
    }

    return S_OK;
}

STDMETHODIMP CDeskBand::ResizeBorderDW(const RECT *, IUnknown *, BOOL)
{
    return E_NOTIMPL;
}

//
// IDeskBand
//
STDMETHODIMP CDeskBand::GetBandInfo(DWORD dwBandID, DWORD, DESKBANDINFO *pdbi)
{
    HRESULT hr = E_INVALIDARG;

    if (pdbi)
    {
        m_dwBandID = dwBandID;

        if (pdbi->dwMask & DBIM_MINSIZE)
        {
            pdbi->ptMinSize.x = 48;
            pdbi->ptMinSize.y = 24;
        }

        if (pdbi->dwMask & DBIM_MAXSIZE)
        {
            pdbi->ptMaxSize.y = -1;
        }

        if (pdbi->dwMask & DBIM_INTEGRAL)
        {
            pdbi->ptIntegral.y = 1;
        }

        if (pdbi->dwMask & DBIM_ACTUAL)
        {
            pdbi->ptActual.x = 48;
            pdbi->ptActual.y = 24;
        }

        if (pdbi->dwMask & DBIM_TITLE)
        {
            // Don't show title by removing this flag.
            pdbi->dwMask &= ~DBIM_TITLE;
        }

        if (pdbi->dwMask & DBIM_MODEFLAGS)
        {
            pdbi->dwModeFlags = DBIMF_NORMAL | DBIMF_VARIABLEHEIGHT;
        }

        if (pdbi->dwMask & DBIM_BKCOLOR)
        {
            // Use the default background color by removing this flag.
            pdbi->dwMask &= ~DBIM_BKCOLOR;
        }

        hr = S_OK;
    }

    return hr;
}

//
// IDeskBand2
//
STDMETHODIMP CDeskBand::CanRenderComposited(BOOL *pfCanRenderComposited)
{
    *pfCanRenderComposited = TRUE;

    return S_OK;
}

STDMETHODIMP CDeskBand::SetCompositionState(BOOL fCompositionEnabled)
{
    m_fCompositionEnabled = fCompositionEnabled;

    InvalidateRect(m_hwnd, NULL, TRUE);
    UpdateWindow(m_hwnd);

    return S_OK;
}

STDMETHODIMP CDeskBand::GetCompositionState(BOOL *pfCompositionEnabled)
{
    *pfCompositionEnabled = m_fCompositionEnabled;

    return S_OK;
}

//
// IPersist
//
STDMETHODIMP CDeskBand::GetClassID(CLSID *pclsid)
{
    *pclsid = CLSID_DeskBand;
    return S_OK;
}

//
// IPersistStream
//
STDMETHODIMP CDeskBand::IsDirty()
{
    return m_fIsDirty ? S_OK : S_FALSE;
}

STDMETHODIMP CDeskBand::Load(IStream * /*pStm*/)
{
    return S_OK;
}

STDMETHODIMP CDeskBand::Save(IStream * /*pStm*/, BOOL fClearDirty)
{
    if (fClearDirty)
    {
        m_fIsDirty = FALSE;
    }

    return S_OK;
}

STDMETHODIMP CDeskBand::GetSizeMax(ULARGE_INTEGER * /*pcbSize*/)
{
    return E_NOTIMPL;
}

//
// IObjectWithSite
//
STDMETHODIMP CDeskBand::SetSite(IUnknown *pUnkSite)
{
    HRESULT hr = S_OK;

    m_hwndParent = NULL;

    if (m_pSite)
    {
        m_pSite->Release();
    }

    if (pUnkSite)
    {
        IOleWindow *pow;
        hr = pUnkSite->QueryInterface(IID_IOleWindow, reinterpret_cast<void **>(&pow));
        if (SUCCEEDED(hr))
        {
            hr = pow->GetWindow(&m_hwndParent);
            if (SUCCEEDED(hr))
            {
                WNDCLASSW wc = { 0 };
                wc.style         = CS_HREDRAW | CS_VREDRAW;
                wc.hCursor       = LoadCursor(NULL, IDC_ARROW);
                wc.hInstance     = g_hInst;
                wc.lpfnWndProc   = WndProc;
                wc.lpszClassName = g_szDeskBandClass;
                wc.hbrBackground = CreateSolidBrush(RGB(255, 255, 0));

                RegisterClassW(&wc);

                CreateWindowExW(0,
                                g_szDeskBandClass,
                                NULL,
                                WS_CHILD | WS_CLIPCHILDREN | WS_CLIPSIBLINGS,
                                0,
                                0,
                                0,
                                0,
                                m_hwndParent,
                                NULL,
                                g_hInst,
                                this);

                if (!m_hwnd)
                {
                    hr = E_FAIL;
                }
            }

            pow->Release();
        }

        hr = pUnkSite->QueryInterface(IID_IInputObjectSite, reinterpret_cast<void **>(&m_pSite));
    }

    return hr;
}

STDMETHODIMP CDeskBand::GetSite(REFIID riid, void **ppv)
{
    HRESULT hr = E_FAIL;

    if (m_pSite)
    {
        hr =  m_pSite->QueryInterface(riid, ppv);
    }
    else
    {
        *ppv = NULL;
    }

    return hr;
}

//
// IInputObject
//
STDMETHODIMP CDeskBand::UIActivateIO(BOOL fActivate, MSG *)
{
    if (fActivate)
    {
        SetFocus(m_hwnd);
    }

    return S_OK;
}

STDMETHODIMP CDeskBand::HasFocusIO()
{
    return m_fHasFocus ? S_OK : S_FALSE;
}

STDMETHODIMP CDeskBand::TranslateAcceleratorIO(MSG *)
{
    return S_FALSE;
};

void CDeskBand::OnFocus(const BOOL fFocus)
{
    m_fHasFocus = fFocus;

    if (m_pSite)
    {
        m_pSite->OnFocusChangeIS(static_cast<IOleWindow*>(this), m_fHasFocus);
    }
}

void CDeskBand::OnPaint(const HDC hdcIn)
{
    HDC hdc = hdcIn;
    PAINTSTRUCT ps;

    if (!hdc)
    {
        hdc = BeginPaint(m_hwnd, &ps);
    }

    if (hdc)
    {
        RECT rc;
        GetClientRect(m_hwnd, &rc);

		Graphics g(hdc);

		if (m_isMouseDown) 
		{
			g.Clear(Color(35, 255, 255, 255));
		}
		else if (m_isMouseOver)
		{
			g.Clear(Color(25, 255, 255, 255));
		}
		else
		{
			g.Clear(Color(255, 0, 0, 0));
		}

		SolidBrush fg(Color(255, 255, 255));
		Pen pen(&fg, 1);

		int contentWidth = 16;
		int contentHeight = 16;
		int contentX = (rc.right - rc.left) / 2 - contentWidth / 2;
		int contentY = (rc.bottom - rc.top) / 2 - contentHeight / 2;

		{
			Point pt1(contentX, contentY)
				, pt2(pt1.X + contentWidth, pt1.Y)
				, pt3(pt2.X, pt2.Y + contentHeight);

			g.DrawLine(&pen, pt1, pt2);
			g.DrawLine(&pen, pt2, pt3);
		}

		{
			int margin = 5;
			Rect rect(
				contentX, contentY + margin, 
				contentHeight - margin, contentWidth - margin);

			g.DrawRectangle(&pen, rect);
		}
    }

    if (!hdcIn)
    {
        EndPaint(m_hwnd, &ps);
    }
}

void CDeskBand::OnClick()
{
	const int MC_DESKBAND_CLICK = WM_APP + 1;
	HWND hTargetWnd = FindWindow(L"MULTICLIP_IPC", NULL);
	if (hTargetWnd)
	{
		PostMessage(hTargetWnd, MC_DESKBAND_CLICK, NULL, NULL);
	}
	else
	{
		MessageBox(NULL, L"MultiClip is not running!", L"Warning", MB_OK | MB_ICONWARNING);
		return;
		// TODO
		HKEY hKey;
		if (RegOpenKey(HKEY_CURRENT_USER, L"Software\\MultiClip", &hKey))
		{
			DWORD dwType = REG_SZ;
			DWORD cbData = MAX_PATH;
			char data[MAX_PATH];
			if (RegQueryValueEx(hKey, L"AppPath", NULL, &dwType, (LPBYTE)&data, &cbData)) 
			{

			}
		}
	}
}

LRESULT CALLBACK CDeskBand::WndProc(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
    LRESULT lResult = 0;

    CDeskBand *pDeskBand = reinterpret_cast<CDeskBand *>(GetWindowLongPtr(hwnd, GWLP_USERDATA));

    switch (uMsg)
    {
    case WM_CREATE:
        pDeskBand = reinterpret_cast<CDeskBand *>(reinterpret_cast<CREATESTRUCT *>(lParam)->lpCreateParams);
        pDeskBand->m_hwnd = hwnd;
        SetWindowLongPtr(hwnd, GWLP_USERDATA, reinterpret_cast<LONG_PTR>(pDeskBand));
        break;

    case WM_SETFOCUS:
        pDeskBand->OnFocus(TRUE);
        break;

    case WM_KILLFOCUS:
        pDeskBand->OnFocus(FALSE);
        break;

    case WM_PAINT:
        pDeskBand->OnPaint(NULL);
        break;

    case WM_PRINTCLIENT:
        pDeskBand->OnPaint(reinterpret_cast<HDC>(wParam));
        break;

    case WM_ERASEBKGND:
        if (pDeskBand->m_fCompositionEnabled)
        {
            lResult = 1;
        }
        break;
    }

    if (uMsg != WM_ERASEBKGND)
    {
        lResult = DefWindowProc(hwnd, uMsg, wParam, lParam);
    }

    return lResult;
}

LRESULT CDeskBand::MouseHookProc(int nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode >= 0 && s_inst && s_inst->m_hwnd && (WM_MOUSEMOVE == wParam || WM_LBUTTONDOWN == wParam || WM_LBUTTONUP == wParam))
	{
		POINT pt;
		RECT rc;
		bool isMouseOver = GetCursorPos(&pt) 
			&& GetWindowRect(s_inst->m_hwnd, &rc) 
			&& PtInRect(&rc, pt);

		if (WM_MOUSEMOVE == wParam)
		{
			if (s_inst->m_isMouseOver != isMouseOver)
			{
				InvalidateRect(s_inst->m_hwnd, NULL, false);
			}
			s_inst->m_isMouseOver = isMouseOver;
			if (!isMouseOver) 
			{
				s_inst->m_isMouseDown = false;
			}
		}
		else if (WM_LBUTTONDOWN == wParam)
		{
			if (s_inst->m_isMouseDown != isMouseOver)
			{
				InvalidateRect(s_inst->m_hwnd, NULL, false);
			}
			s_inst->m_isMouseDown = isMouseOver;
			if (isMouseOver)
			{
				s_inst->OnClick();
				return 1;
			}
		}
		else if (WM_LBUTTONUP == wParam)
		{
			if (s_inst->m_isMouseDown != false) 
			{
				InvalidateRect(s_inst->m_hwnd, NULL, false);
			}
			s_inst->m_isMouseDown = false;
		}

	}

	return CallNextHookEx(NULL, nCode, wParam, lParam);
}
