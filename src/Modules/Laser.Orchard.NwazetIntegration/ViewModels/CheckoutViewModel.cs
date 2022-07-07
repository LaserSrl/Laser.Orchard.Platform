using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Newtonsoft.Json;
using Nwazet.Commerce.Models;
using Nwazet.Commerce.Services;
using Orchard.ContentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;

namespace Laser.Orchard.NwazetIntegration.ViewModels {
    public class CheckoutViewModel {
        public CheckoutViewModel() {
            ListAvailableShippingAddress = new List<AddressRecord>();
            ListAvailableBillingAddress = new List<AddressRecord>();

            AvailableShippingOptions = new List<ShippingOption>();

            ProviderViewModels = new Dictionary<string, object>();
        }

        /// <summary>
        /// Used in CheckoutController actions to signal that we should shortcircuit
        /// where we can and move forward towards payment
        /// </summary>
        public bool UseDefaultAddress { get; set; }
        public bool UseDefaultShipping { get; set; }
        public bool BillAtSameShippingAddress { get; set; }

        [JsonIgnore]
        public ICurrencyProvider CurrencyProvider;

        #region Cart

        [JsonIgnore]
        public IShoppingCart ShoppingCart;

        [JsonIgnore]
        public IProductPriceService ProductPriceService;

        public IEnumerable<ShoppingCartQuantityProduct> GetProductQuantities() {
            return ShoppingCart
                .GetProducts()
                .Where(p => p.Product != null && p.Quantity > 0);
        }

        public decimal GetPrice(ShoppingCartQuantityProduct productQuantity) {
            return ProductPriceService.GetPrice(
                productQuantity.Product,
                ShoppingCart?.Country,
                ShoppingCart?.ZipCode);
        }
        public decimal GetOriginalPrice(ShoppingCartQuantityProduct productQuantity) {
            return ProductPriceService.GetPrice(
                productQuantity.Product,
                ShoppingCart?.Country,
                ShoppingCart?.ZipCode);
        }
        public decimal GetDiscountedPrice(ShoppingCartQuantityProduct productQuantity) {
            return ProductPriceService.GetPrice(
                productQuantity.Product,
                productQuantity.Price,
                ShoppingCart?.Country,
                ShoppingCart?.ZipCode);
        }
        public decimal GetLinePriceAdjustement(ShoppingCartQuantityProduct productQuantity) {
            return ProductPriceService.GetPrice(
                productQuantity.Product,
                productQuantity.LinePriceAdjustment,
                ShoppingCart?.Country,
                ShoppingCart?.ZipCode);
        }
        
        public decimal GetShoppingTotal() {
            return ShoppingCart.Total(
                ShoppingCart.Subtotal(),
                ShoppingCart.Taxes());
        }
        #endregion

        #region Addresses
        // Information in this region is modified in the Index Actions CheckoutController

        public AddressesVM AsAddressesVM() {
            return new AddressesVM {
                ShippingAddress = ShippingAddress,
                ShippingAddressVM = ShippingAddressVM,
                BillingAddress = BillingAddress,
                BillingAddressVM = BillingAddressVM,
                Email = Email,
                PhonePrefix = PhonePrefix,
                Phone = Phone,
                SpecialInstructions = SpecialInstructions,
                ListAvailableShippingAddress = ListAvailableShippingAddress,
                ListAvailableBillingAddress = ListAvailableBillingAddress,
                ProviderViewModels = ProviderViewModels
            };
        }
        public void SetAddressesVM(AddressesVM vm) {
            ShippingAddress = vm.ShippingAddress;
            ShippingAddressVM = vm.ShippingAddressVM;
            BillingAddress = vm.BillingAddress;
            BillingAddressVM = vm.BillingAddressVM;
            Email = vm.Email;
            PhonePrefix = vm.PhonePrefix;
            Phone = vm.Phone;
            SpecialInstructions = vm.SpecialInstructions;
            ListAvailableShippingAddress = vm.ListAvailableShippingAddress;
            ListAvailableBillingAddress = vm.ListAvailableBillingAddress;

            ProviderViewModels = vm.ProviderViewModels;
        }

        public Address ShippingAddress { get; set; }
        public AddressEditViewModel ShippingAddressVM { get; set; }
        // used to carry over selected address from the form
        // property should not be changed cause its editor is manually generated in AddressForm.cshtml
        public int ShippingAddressVMListAddress { get; set; }

        // These shapes allow using PickupPoints and such
        public string SelectedShippingAddressProviderId { get; set; }

        [JsonIgnore]
        public ICheckoutShippingAddressProvider SelectedShippingAddressProvider { get; set; }
        // Each provider may need its own view model to manage what it's displaying. It's the provider's
        // responsibility to cast and handle those objects responsibly.
        public Dictionary<string, object> ProviderViewModels { get; set; }

        [JsonIgnore]
        public IEnumerable<AdditionalIndexShippingAddressViewModel> AdditionalShippingAddressShapes { get; set; }

        public Address BillingAddress { get; set; }
        public AddressEditViewModel BillingAddressVM { get; set; }
        // used to carry over selected address from the form
        // property should not be changed cause its editor is manually generated in AddressForm.cshtml
        [JsonIgnore]
        public int BillingAddressVMListAddress { get; set; }

        public string Email { get; set; }
        public string PhonePrefix { get; set; }
        public string Phone { get; set; }
        public string SpecialInstructions { get; set; }
        [JsonIgnore]
        public List<AddressRecord> ListAvailableShippingAddress { get; set; }

        [JsonIgnore]
        public List<AddressRecord> ListAvailableBillingAddress { get; set; }

        private const string AddressEncryptionPurpose = "Serialize Address Information";

        [JsonIgnore]
        public string SerializedAddresses { get; set; }
        public string EncodeAddresses() {

            SerializedAddresses = Convert.ToBase64String(
                MachineKey.Protect(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(AsAddressesVM())),
                    AddressEncryptionPurpose
                    ));
            return SerializedAddresses;
        }
        public void DecodeAddresses() {
            var bytes = Convert.FromBase64String(SerializedAddresses);
            var unprotected = MachineKey.Unprotect(bytes, AddressEncryptionPurpose);
            if (unprotected != null) {
                SetAddressesVM(
                    JsonConvert.DeserializeObject<AddressesVM>(
                        Encoding.UTF8.GetString(unprotected)));
            }
        }
        public bool ResetAddresses { get; set; }
        #endregion

        #region Shipping
        [JsonIgnore]
        public bool ShippingRequired { get; set; }
        [JsonIgnore]
        public List<ShippingOption> AvailableShippingOptions { get; set; }
        /// <summary>
        /// This is ShippingOption.FormValue for the selected option, used to pull it
        /// from the form. This is called "ShippingOption" for retrocompatibility and
        /// to allow reusing pre-existing shapes.
        /// </summary>
        public string ShippingOption { get; set; }
        public ShippingOption SelectedShippingOption { get; set; }
        public bool ResetShipping { get; set; }
        #endregion

        #region Payments
        public IEnumerable<IPosService> PosServices { get; set; }
        /// <summary>
        /// Will contian the name of the selected PosService when the user
        /// selects it from the form.
        /// </summary>
        public string SelectedPosService { get; set; }
        #endregion

        public static void ReinflateViewModelAddresses(
            CheckoutViewModel vm, IContentManager contentManager, IAddressConfigurationService addressConfigurationService) {
            // addresses
            if ((vm.ShippingAddressVM == null || vm.BillingAddressVM == null)
                && !string.IsNullOrWhiteSpace(vm.SerializedAddresses)) {
                vm.DecodeAddresses();
            }
            Func<string, int, string> inflateName = (str, id) => {
                if (string.IsNullOrWhiteSpace(str)) {
                    var territory = addressConfigurationService
                        .SingleTerritory(id);
                    if (territory != null) {
                        return contentManager
                            .GetItemMetadata(territory).DisplayText;
                    }
                }
                return str;
            };
            if (vm.ShippingAddressVM != null) {
                if (vm.ShippingAddress == null) {
                    vm.ShippingAddress = vm.ShippingAddressVM.MakeAddressFromVM();
                }
                // reinflate the names of country, province and city
                vm.ShippingAddressVM.Country = inflateName(
                    vm.ShippingAddressVM.Country, vm.ShippingAddressVM.CountryId);
                vm.ShippingAddressVM.Province = inflateName(
                    vm.ShippingAddressVM.Province, vm.ShippingAddressVM.ProvinceId);
                vm.ShippingAddressVM.City = inflateName(
                    vm.ShippingAddressVM.City, vm.ShippingAddressVM.CityId);
            }
            if (vm.BillingAddressVM != null) {
                if (vm.BillingAddress == null) {
                    vm.BillingAddress = vm.BillingAddressVM.MakeAddressFromVM();
                }
                // reinflate the names of country, province and city
                vm.BillingAddressVM.Country = inflateName(
                    vm.BillingAddressVM.Country, vm.BillingAddressVM.CountryId);
                vm.BillingAddressVM.Province = inflateName(
                    vm.BillingAddressVM.Province, vm.BillingAddressVM.ProvinceId);
                vm.BillingAddressVM.City = inflateName(
                    vm.BillingAddressVM.City, vm.BillingAddressVM.CityId);
            }
        }
        
        // TODO: use these methods to serialize/deserialize the entire viewmodel
        // rather than just the addresses. This way we carry also the information 
        // Selections at different steps.
        public static string EncodeCheckoutObject(CheckoutViewModel cvm) {
            return Convert.ToBase64String(
                MachineKey.Protect(
                    Encoding.UTF8.GetBytes(
                        JsonConvert.SerializeObject(cvm)),
                    AddressEncryptionPurpose
                    ));
        }
        public static CheckoutViewModel DecodeCheckoutObject(string str) {
            var bytes = Convert.FromBase64String(str);
            var unprotected = MachineKey.Unprotect(bytes, AddressEncryptionPurpose);
            if (unprotected != null) {
                return
                    JsonConvert.DeserializeObject<CheckoutViewModel>(
                        Encoding.UTF8.GetString(unprotected));
            }
            return null;
        }
    }
}