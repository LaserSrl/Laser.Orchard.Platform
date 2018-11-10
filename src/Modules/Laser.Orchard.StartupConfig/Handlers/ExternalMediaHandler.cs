using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.Utilities;
using Orchard.MediaLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;

namespace Laser.Orchard.StartupConfig.Handlers {
    public class ExternalMediaHandler : ContentHandler {

        public ExternalMediaHandler() {

            OnLoaded<MediaPart>((context, part) => {
                if (!String.IsNullOrEmpty(part.FileName)) {
                    //regex pattern for http or https
                    //^(https?):\/\/\S+[.]\S+
                    //check whether the filename points to a remote resource
                    if (Regex.IsMatch(part.FileName, @"^(https?):\/\/\S+[.]\S+")) {
                        BindingFlags bindFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
        | BindingFlags.Static;
                        FieldInfo field = part.GetType().GetField("_publicUrl", bindFlags);
                        ((LazyField<string>)field.GetValue(part)).Loader(() => part.FileName);
                    }
                }
            });
        }
    }
}