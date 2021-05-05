namespace Laser.Orchard.SecureData.Settings {
    public class EncryptedStringFieldSettings {
        public string Hint { get; set; }
        public bool Required { get; set; }
        public string Pattern { get; set; }
        /// <summary>
        /// Show the Confirm input box.
        /// </summary>
        public bool ConfirmRequired { get; set; }
        /// <summary>
        /// IsVisible = True -> show decrypted value to authorized users.
        /// IsVisible = False -> never show decrypted value.
        /// </summary>
        public bool IsVisible { get; set; }
    }
}