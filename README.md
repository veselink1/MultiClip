# MultiClip - copy and paste unleashed
### Contributers - veselink1, lubomarinski

## MultiClip allows users to unlock the full potential of the Clipboard. 

## Features
• Store multiple items in the clipboard and access the from anywhere
• Share clipboard items across network devices
• In-memory clipboard item compression for large items
• Supports a large variety of formats, including (plain text, rich text, images, file drops and more)
• Uses a simple and fast interface
• Does *not* steal focus when the window is opened and allows for uninterrupted work experience 
• Entirely customizable (choose a theme, change the accent color, and toggle transparency and blur)
• Both keyboard and touch-optimized, to allow to be used on a variety of devices
• Uses efficient low-level Win32 APIs to optimize for performace and memory
• Optionally use the deskband, which is an Explorer extension that blends into the taskbar
• The application is multilingual (currently supports English and Bulgarian)
• Checking for updates and newer versions of the app (WIP)
• User-specified system memory usage limit, to limit the number of items stored in RAM (WIP)
• Explicit exclusion of the current clipboard state from the list

## Screenshots

![Image 1](https://raw.githubusercontent.com/veselink1/MultiClip/master/Screenshots/Screenshot%20(123).png)
![Image 2](https://raw.githubusercontent.com/veselink1/MultiClip/master/Screenshots/Screenshot%20(124).png)

## Information
The core application is written in C# and uses the WPF framework. All the controls are customized to reflect the UWP (Acrylic) design Microsft is promoting with Windows 10. The application makes use of low-level Windows APIs, unsafe C# code, WeakReferences and in-memory compression through UnmanagedMemoryStream to increase throughput and performance, reduce memory requirements and minimize stress on the .NET GC. The collection of data from the clipboard is simultaneous and uninterrupting. Restoring a previous clipboard state is also efficient. 

When saving a clipboard state to memory, not only the primary, but also all secondary formats are stored. This would generally introduce a large memory hit, as many programs create lazy "proxies" for all common formats. MultiClip tries to deduplicate the formats that can be losslessly converted in one another and does not store multiple copies of such data. Other programs, like Microsoft Excel and LibreOffice Calc store cell data in CSV, PNG, JPEG and many other formats which is also problematic. MultiClip solves this problem by taking advantage of Windows' lazy conversion system and MultiClip's in-memory streaming compression system to reduce the footprint of images and other large data sometimes by and order of magnitude, which essentially solves the problem that these multiple formats create. Since these operations are streaming (i.e. they read raw memory and emit compressed bytes without allocating and copying the data in intermediate blocks), the copy operation does not overflow the available RAM as some other similar applications do.

Another feature that MultiClip has that other similar applications lack is the ability to not steal focus from the opened application. The main window is styled with the WS_EX_NOACTIVATE, WS_EX_TOPMOST and WS_EX_TOOLWINDOW extended window styled, which allows it to respond to events like mouse click and other such interaction without the currently opened window losing focus, which in some applications might mean losing the selection are or the position where the data is to be pasted. 

### MultiClip Deskband
The deskband is an Explorer.exe extension written in regular C++. It binds to the taskbar surface area and displays an additional icon that allows the main MultiClip window to appear, again without stealing the focus (from the opened window to the taskbar).

### UComponents
UComponent is a bare-bones WPF UI library that contains some custom controls that mimick the ones found in the UWP platform. These controls include a Button, Checkbox, RadioButton, ToggleButton, Slider, TextBox and a PasswordBox. All controls support the dynamic theme switching capabilities that the library implements. Some other utility features are a SystemAccentColor provided which allows runtime tracking of accent color changes from the Windows 10 settings app (through hooking to the WM_DWMCOLORIZATIONCOLORCHANGED event) and support for BlurBehind in Windows 7 as well as Windows 10 (Windows 8 is not supported).
