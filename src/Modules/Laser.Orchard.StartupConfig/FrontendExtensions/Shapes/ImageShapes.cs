using Orchard;
using Orchard.ContentManagement;
using Orchard.DisplayManagement;
using Orchard.Environment;
using Orchard.Forms.Services;
using Orchard.Logging;
using Orchard.MediaLibrary.Models;
using Orchard.MediaProcessing.Models;
using Orchard.MediaProcessing.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Web.Mvc;

namespace Laser.Orchard.StartupConfig.FrontendExtensions.Shapes {
    public class ImageShapes : IDependency {
        private readonly Work<IImageProfileManager> _imageProfileManager;

        public ImageShapes(
            Work<IImageProfileManager> imageProfileManager) {

            _imageProfileManager = imageProfileManager;
        }

        public ILogger Logger { get; set; }

        /// <summary>
        /// This method allows the configuration of a img html tag. It can use a MediaPart, an ImagePart
        /// or the path to an image file. It can resize the image. It can also be configured to set up
        /// attributes for lazy load of the image: the thumbnail can be configured in many ways.
        /// </summary>
        /// <param name="Shape"></param>
        /// <param name="Display"></param>
        /// <param name="Output"></param>
        /// <param name="MediaPart">The MediaPart to be used to create an image tag.</param>
        /// <param name="ImagePart">The ImagePart to be used to create an image tag.</param>
        /// <param name="ImagePath">The path to the source image for the image tag.</param>
        /// <param name="Width">The width we will attempt using for the image profile used in the tag.</param>
        /// <param name="Height">The height we will attempt using for the image profile used in the tag.</param>
        /// <param name="Mode">Resize mode for the image.</param>
        /// <param name="Alignment">Alignment for the resized image.</param>
        /// <param name="PadColor">Color to use when padding the image.</param>
        /// <param name="alt">This parameter will be used for the alt attribute of the image tag in case
        /// the MediaPart's is empty, or there is no MediaPart.</param>
        /// <param name="title">This parameter will be used for the title attribute of the image tag in case
        /// the MediaPart's is empty, or there is no MediaPart.</param>
        /// <param name="StubMediaPart">The MediaPart to be used for the thumbnail of the image tag.</param>
        /// <param name="StubImagePart">The ImagePart to be used for the thumbnail of the image tag.</param>
        /// <param name="StubImagePath">The path to the source image for the thumbnail of the image tag.</param>
        /// <param name="StubWidth">The width we will attempt using for the image profile used in the tag.</param>
        /// <param name="StubHeight">The height we will attempt using for the image profile used in the tag.</param>
        /// <param name="StubMode">Resize mode for the image.</param>
        /// <param name="StubAlignment">Alignment for the resized image.</param>
        /// <param name="StubPadColor">Color to use when padding the image.</param>
        /// <param name="htmlAttributes">Additional html attributes for the img tag. This parameter is used, for example
        /// to set the css class for the image.</param>
        /// <remarks>The source image is chosen between a ContentItem or a path to an image. In the latter
        /// case, the Width and Height parameters must both be greater than 0. It is possible to use the main
        /// image as a source for the stub: to do this, don't set StubMediaPart, StubImagePart and StubImagePath,
        /// but provide positive values for StubWidth and StubHeight.</remarks>
        [Shape]
        public void ImageTag(
            dynamic Shape,
            dynamic Display,
            TextWriter Output,
            /* Parameters for the image we want to show */
            MediaPart MediaPart,
            ImagePart ImagePart,
            string ImagePath,
            int Width,
            int Height,
            string Mode,
            string Alignment,
            string PadColor,
            string alt,
            string title,
            /* Parameters for the thumbnail used for lazy loading of image */
            MediaPart StubMediaPart,
            ImagePart StubImagePart,
            string StubImagePath,
            int StubWidth,
            int StubHeight,
            string StubMode,
            string StubAlignment,
            string StubPadColor,
            IDictionary<string, object> htmlAttributes) {

            ImageInfo mainImage = ImageInfo.New(MediaPart, ImagePart, Width, Height);

            if (mainImage == null) {
                // attempt to validate the information without using the ContentItem
                mainImage = ImageInfo.New(ImagePath, Width, Height);
            }

            if (mainImage == null) {
                Logger.Error("It was impossible to figure out the image to render.");
                return;
            }

            // At this point, mainImage contains all the information to render the image
            if (MediaPart != null) {
                // The MediaPart may have its own alternate text and title
                title = string.IsNullOrWhiteSpace(MediaPart.Title) ? title : MediaPart.Title;
                alt = string.IsNullOrWhiteSpace(MediaPart.AlternateText) ? alt : MediaPart.AlternateText;
            }

            // Check whether we are also trying to render a thumbnail/stub for lazy loading
            // thumbnail cannot be larger than full image:
            StubWidth = StubWidth > mainImage.Width ? mainImage.Width : StubWidth;
            // thumbnail cannot be taller than full image:
            StubHeight = StubHeight > mainImage.Height ? mainImage.Height : StubHeight;
            ImageInfo stubImage = ImageInfo.New(StubMediaPart, StubImagePart, StubWidth, StubHeight);
            if (stubImage == null) {
                stubImage = ImageInfo.New(StubImagePath, StubWidth, StubHeight);
            }
            // Lastly, we may want to generate a stub base on the main image:
            if (stubImage == null && StubWidth > 0 && StubHeight > 0) {
                stubImage = new ImageInfo() {
                    ContentItem = mainImage.ContentItem,
                    ImagePath = mainImage.ImagePath,
                    Width = StubWidth,
                    Height = StubHeight
                };
            }

            // Generate tag, with attributes that are common to both normal and lazyload cases
            TagBuilder tagBuilder = new TagBuilder("img");
            tagBuilder.MergeAttributes(htmlAttributes); // this for example allows adding a class
            if (!string.IsNullOrWhiteSpace(title)) {
                tagBuilder.MergeAttribute("title", title);
            }
            if (!string.IsNullOrWhiteSpace(alt)) {
                tagBuilder.MergeAttribute("alt", alt);
            }
            // Generate stuff needed for the image profiles
            var imagePath = GetProfileUrl(mainImage, Mode, Alignment, PadColor);
            if (stubImage == null) {
                // "normal" behaviour, without stub image for lazyload
                tagBuilder.MergeAttribute("src", imagePath);
            } else {
                // stub settings:
                StubMode = string.IsNullOrWhiteSpace(StubMode) ? Mode : StubMode;
                StubAlignment = string.IsNullOrWhiteSpace(StubAlignment) ? Alignment : StubAlignment;
                StubPadColor = string.IsNullOrWhiteSpace(StubPadColor) ? PadColor : StubPadColor;
                // generate tag with stub image for lazyload
                var imageStub = GetProfileUrl(stubImage, StubMode, StubAlignment, StubPadColor);
                tagBuilder.MergeAttribute("src", imageStub);
                tagBuilder.MergeAttribute("data-src", imagePath);
            }

            Output.Write(tagBuilder.ToString(TagRenderMode.Normal));
        }

        private string GetProfileUrl(
            MediaPart mediaPart,
            int Width,
            int Height,
            string Mode,
            string Alignment,
            string PadColor) {

            return GetProfileUrl(mediaPart.MediaUrl, Width, Height, Mode, Alignment, PadColor, mediaPart.ContentItem);
        }

        private string GetProfileUrl(
            string Path,
            int Width,
            int Height,
            string Mode,
            string Alignment,
            string PadColor,
            ContentItem contentItem = null) {

            var filter = Filter(Width, Height, Mode, Alignment, PadColor);

            var profile = Profile(Width, Height, Mode, Alignment, PadColor);

            return GetProfileUrl(Path, profile, filter, contentItem);
        }

        private string GetProfileUrl(
            ImageInfo imageInfo,
            string Mode,
            string Alignment,
            string PadColor) {

            var filter = Filter(imageInfo.Width, imageInfo.Height, Mode, Alignment, PadColor);
            var profile = Profile(imageInfo.Width, imageInfo.Height, Mode, Alignment, PadColor);

            return GetProfileUrl(imageInfo.ImagePath, profile, filter, imageInfo.ContentItem);
        }

        private string GetProfileUrl(string Path, string Profile, FilterRecord CustomFilter, ContentItem ContentItem) {
            return _imageProfileManager.Value.GetImageProfileUrl(Path, Profile, CustomFilter, ContentItem);
        }

        private string Profile(
            int Width,
            int Height,
            string Mode,
            string Alignment,
            string PadColor) {
            return "Transform_Resize"
                + "_w_" + Convert.ToString(Width)
                + "_h_" + Convert.ToString(Height)
                + "_m_" + Convert.ToString(Mode)
                + "_a_" + Convert.ToString(Alignment)
                + "_c_" + Convert.ToString(PadColor);
        }

        private FilterRecord Filter(
            int Width,
            int Height,
            string Mode,
            string Alignment,
            string PadColor) {

            var state = new Dictionary<string, string> {
                {"Width", Width.ToString(CultureInfo.InvariantCulture)},
                {"Height", Height.ToString(CultureInfo.InvariantCulture)},
                {"Mode", Mode},
                {"Alignment", Alignment},
                {"PadColor", PadColor},
            };

            return new FilterRecord {
                Category = "Transform",
                Type = "Resize",
                State = FormParametersHelper.ToString(state)
            };
        }

        /// <summary>
        /// Class used in the methods here to represent an Image to be rendered
        /// </summary>
        class ImageInfo {
            public ContentItem ContentItem { get; set; }
            public string ImagePath { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }

            public static ImageInfo New(
                MediaPart mediaPart,
                ImagePart imagePart,
                int width,
                int height) {


                if (mediaPart == null) {
                    mediaPart = imagePart.As<MediaPart>();
                } else if (imagePart == null) {
                    imagePart = mediaPart.As<ImagePart>();
                }

                // at minimum, we need a MediaPart to get to a file
                if (mediaPart == null) {
                    return null;
                }

                if (imagePart != null) {
                    // Manage Width <= 0, Height <= 0
                    if (width <= 0) {
                        width = imagePart.Width;
                    }
                    if (height <= 0) {
                        height = imagePart.Height;
                    }
                }

                var imagePath = mediaPart.MediaUrl;

                var img = New(imagePath, width, height);
                if (img != null) {
                    img.ContentItem = mediaPart.ContentItem;
                }
                return img;
            }

            public static ImageInfo New(
                string imagePath,
                int width,
                int height) {

                // validate information
                if (string.IsNullOrWhiteSpace(imagePath)
                    || width <= 0 || height <= 0) {
                    return null;
                }

                var img = new ImageInfo() {
                    ContentItem = null,
                    ImagePath = imagePath,
                    Width = width,
                    Height = height
                };
                return img;
            }
        }

    }
}