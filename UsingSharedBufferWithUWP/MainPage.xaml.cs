using Microsoft.Web.WebView2.Core;
using System;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UsingSharedBufferWithUWP
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await WebView2.EnsureCoreWebView2Async();

            WebView2.CoreWebView2.SetVirtualHostNameToFolderMapping(
                "pspdfkit.uwp.example",
                "Assets/html",
                CoreWebView2HostResourceAccessKind.Allow);
            WebView2.Source = new Uri("http://pspdfkit.uwp.example/index.html");
        }

        private async void LoadImageButtonClicked(object sender, RoutedEventArgs e)
        {
            // Show Open File Dialog to select image
            FileOpenPicker fileOpenPicker = new FileOpenPicker();
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".png");
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;
            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            // Store selected image in a buffer
            var buffer = await FileIO.ReadBufferAsync(inputFile);

            var environment = await CoreWebView2Environment.CreateAsync();
            // Create shared buffer
            using (var sharedBuffer = environment.CreateSharedBuffer(buffer.Length))
            {
                using (var stream = sharedBuffer.OpenStream())
                {
                    // Write image to the shared buffer
                    using (DataWriter writer = new DataWriter(stream.GetOutputStreamAt(0)))
                    {
                        writer.WriteBuffer(buffer);
                        await writer.StoreAsync();
                    }

                    string additionalDataAsJson = ""; // can provide some extra information when we share the 
                    // Send and Notify web of shared buffer
                    WebView2.CoreWebView2.PostSharedBufferToScript(sharedBuffer,
                        CoreWebView2SharedBufferAccess.ReadOnly,
                        additionalDataAsJson);
                }
            }
        }
    }
}
