using Laser.Orchard.NwazetIntegration.Models;
using Laser.Orchard.NwazetIntegration.Services;
using Laser.Orchard.NwazetIntegration.Services.CheckoutShippingAddressProviders;
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

        [JsonIgnore]
        public string State { get; set; }
        // use these methods to serialize/deserialize the entire viewmodel
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
                try {
                    return
                        JsonConvert.DeserializeObject<CheckoutViewModel>(
                            Encoding.UTF8.GetString(unprotected));
                } catch {
                    return null;
                }
            }
            return null;
        }

        public void ReiflateState(
            IContentManager contentManager, 
            IAddressConfigurationService addressConfigurationService,
            IEnumerable<ICheckoutShippingAddressProvider> checkoutShippingAddressProviders) {

            // decode what's coming from the form. At each step later we'll compare what we have
            // in the current "this" object, with what we decoded, to see whether there's anything
            // we should be taking from the form.
            var tempVm = DecodeCheckoutObject(State) ??  new CheckoutViewModel();

            // Try to ensure a a shipping address provider is selected
            if (string.IsNullOrWhiteSpace(SelectedShippingAddressProviderId)) {
                SelectedShippingAddressProviderId = tempVm.SelectedShippingAddressProviderId;
            }
            SelectedShippingAddressProvider = checkoutShippingAddressProviders
                .FirstOrDefault(sap => 
                    sap.IsSelectedProviderForIndex(SelectedShippingAddressProviderId));

            #region Reinflate addresses
            // Reinflate "default" addresses from previous state if the current object doesn't have
            // them set already. If they are set already it likely means they've been set by the
            // form or something like that, so we shouldn't overwrite them.

            // Reinflate a territory name if it's not set yet
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

            // Billing address: this is handled here/by the controller directly
            if (BillingAddressVM == null) {
                BillingAddressVM = tempVm.BillingAddressVM;
            }
            if (BillingAddressVM != null) { // it may have never been set
                if (BillingAddress == null) {
                    BillingAddress = BillingAddressVM.MakeAddressFromVM();
                }
                // reinflate the names of country, province and city
                BillingAddressVM.Country = inflateName(
                    BillingAddressVM.Country, BillingAddressVM.CountryId);
                BillingAddressVM.Province = inflateName(
                    BillingAddressVM.Province, BillingAddressVM.ProvinceId);
                BillingAddressVM.City = inflateName(
                    BillingAddressVM.City, BillingAddressVM.CityId);
            }

            // Shipping address: each provider is responsible for its own way of representing 
            // the information it needs for the shipping address, hence each provider will have
            // to be able to handle its own way to reinflate the information from what's serialized.
            if (SelectedShippingAddressProvider != null) {
                SelectedShippingAddressProvider.ReinflateShippingAddress(
                    new ShippingAddressReinflationContext {
                        TargetCheckoutViewModel = this,
                        SourceCheckoutViewModel = tempVm
                    });
            }
            #endregion
        }



        /// <summary>
        /// Final steps for the view model before sending it to a view
        /// </summary>
        public void FinalSetup() {

            if (ShippingAddressVM != null && BillingAddressVM != null &&
                ShippingAddressVM.CountryId == BillingAddressVM.CountryId &&
                ShippingAddressVM.Country == BillingAddressVM.Country &&
                ShippingAddressVM.CityId == BillingAddressVM.CityId &&
                ShippingAddressVM.City == BillingAddressVM.City &&
                ShippingAddressVM.Company == BillingAddressVM.Company &&
                ShippingAddressVM.Address1 == BillingAddressVM.Address1 &&
                ShippingAddressVM.Address2 == BillingAddressVM.Address2 &&
                ShippingAddressVM.FirstName == BillingAddressVM.FirstName &&
                ShippingAddressVM.LastName == BillingAddressVM.LastName &&
                ShippingAddressVM.Honorific == BillingAddressVM.Honorific &&
                ShippingAddressVM.PostalCode == BillingAddressVM.PostalCode &&
                ShippingAddressVM.ProvinceId == BillingAddressVM.ProvinceId &&
                ShippingAddressVM.Province == BillingAddressVM.Province
            ) {
                BillAtSameShippingAddress = true;
            } else {
                BillAtSameShippingAddress = false;
            }

            State = EncodeCheckoutObject(this);
        }

    }
}