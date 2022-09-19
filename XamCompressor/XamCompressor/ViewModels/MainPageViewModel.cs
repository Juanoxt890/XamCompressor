
using DryIoc;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PlatformConfiguration;
using XamCompressor.DependencyServices;
using XamCompressor.Helpers;
using XamCompressor.Models;

namespace XamCompressor.ViewModels
{


    public class MainPageViewModel : ViewModelBase
    {
        #region Fields

        private string _imageSource;
        public string ImageSource
        {
            get => _imageSource;
            set
            {
                SetProperty(ref _imageSource, value);
            }
        }

        private string _oSVersion;
        public string OSVersion
        {
            get => _oSVersion;
            set
            {
                SetProperty(ref _oSVersion, value);
            }
        }
        private string _resizedImageSource;
        public string ResizedImageSource
        {
            get => _resizedImageSource;
            set
            {
                SetProperty(ref _resizedImageSource, value);
            }
        }

        private byte[] image;
        public byte[] Image
        {
            get { return image; }
            set { SetProperty(ref image, value); }
        }

        private byte[] resizedImage;
        public byte[] ResizedImage
        {
            get { return resizedImage; }
            set { SetProperty(ref resizedImage, value); }
        }

        private string _imageSize;
        public string ImageSize
        {
            get => _imageSize;
            set
            {
                SetProperty(ref _imageSize, value);
            }
        }
        private string _resizedImageSize;
        public string ResizedImageSize
        {
            get => _resizedImageSize;
            set
            {
                SetProperty(ref _resizedImageSize, value);
            }
        }

        FileResult ImageSelected = null;
        #endregion

        #region Commands
        public DelegateCommand PickImageCommand { get; set; }
        public DelegateCommand ResizeImageCommand { get; set; }
        #endregion
        public MainPageViewModel(INavigationService navigationService, IPageDialogService pageDialogService)
            : base(navigationService)
        {

            OSVersion = $"DeviceType: {DeviceInfo.DeviceType}, Name: {DeviceInfo.Name}, O.S Version:{DeviceInfo.Version}, VersionString:{DeviceInfo.VersionString}, Manufacturer:{DeviceInfo.Manufacturer}, Type:{DeviceInfo.Idiom}.";
            PickImageCommand = new DelegateCommand(async () =>
            {
                await CheckPermissions();
                ImageSelected = await MediaPicker.PickPhotoAsync();

                if (ImageSelected == null)
                {
                    await pageDialogService.DisplayAlertAsync("Aviso", "No seleccionaste una imagen", "OK");
                    return;
                }
                Image = File.ReadAllBytes(ImageSelected.FullPath);
                ImageSize = BytesToMB(Image.Length).ToString();
                ImageSource = ImageSelected.FullPath;
            });

            ResizeImageCommand = new DelegateCommand(async () =>
            {
                if (Image == null)
                {
                    await pageDialogService.DisplayAlertAsync("Aviso", "No seleccionaste una imagen", "OK");
                    return;
                }
                float mbs = BytesToMB(Image.Length);
                if (mbs < 1)
                {
                    //send file normally
                }
                else
                {
                    if(Device.RuntimePlatform == Device.Android)
                    {
                        ImageOutputModel resizer = await DependencyService.Get<IResizeImageService>().ResizeImage(ImageSelected.FullPath);
                        ResizedImage = resizer.ResizedImageBytes;
                        ResizedImageSource = resizer.ResizedImagePath;
                        ResizedImageSize = BytesToMB(ResizedImage.Length).ToString();
                    }
                    else
                    {
                        ImageOutputModel resizer = await DependencyService.Get<IResizeImageService>().ResizeImage(Image,ImageSelected.FullPath);
                        ResizedImage = resizer.ResizedImageBytes;
                        ResizedImageSource = resizer.ResizedImagePath;
                        ResizedImageSize = BytesToMB(ResizedImage.Length).ToString();
                    }
             
                }
            });
        }

        private static async Task CheckPermissions()
        {
            var readPermission = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var writePermission = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (readPermission == PermissionStatus.Denied)
            {
                await Permissions.RequestAsync<Permissions.StorageRead>();
            }
            if (writePermission == PermissionStatus.Denied)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
        }

        private float BytesToMB(int length) => (length / 1024f) / 1024f;



    }
}
