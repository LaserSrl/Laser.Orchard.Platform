using Laser.Orchard.FidelityGateway.Models;
using Orchard;
using System.Collections.Generic;

namespace Laser.Orchard.FidelityGateway.Services
{
    public interface IFidelityServices : IDependency
    {
        /// <summary>
        /// Crea un account sul provider remoto basandosi sui dati dell'utente orchard autenticato, 
        /// associando l'utente alla campagna di default
        /// </summary>
        /// <returns>APIResult con incapsulato il FidelityCustomer con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCustomer> CreateFidelityAccountFromCookie();

        /// <summary>
        /// Crea un account sul provider remoto con i dati passati per parametro
        /// </summary>
        /// <returns>APIResult con incapsulato il FidelityCustomer con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCustomer> CreateFidelityAccount(FidelityUserPart fidelityPart, string username, string email, string camapaignId);

        /// <summary>
        /// Richiede tutti i dettagli della fidelity di un utente autenticato in orchard
        /// </summary>
        /// <returns>APIResult con incapsulato il FidelityCustomer con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCustomer> GetCustomerDetails(string customerId);

        /// <summary>
        /// Richiede tutti i dettagli di una certa campagna di fidelizzazione
        /// </summary>
        /// <param name="id">Id della campagna per la quale si vogliono i dettagli </param>
        /// <returns>APIResult con incapsulato il FidelityCampaign con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCampaign> GetCampaignData(string id);

        /// <summary>
        /// Richiede l'aggiunta di punti del cliente autenticato in Orchard su una determinata campagna ad un determinato utunte.
        /// </summary>
        /// <param name="numPoints">numero di punti da aumentare </param>
        /// <param name="campaignId">identificativo della campagna su cui aumentare i punti </param>
        /// <param name="customerId">identificativo (id o email) del cliente a cui aumentare i punti </param>
        /// <returns>APIResult con incapsulato id campagna, id cliente e il numero di punti aggiornato</returns>  
        APIResult<CardPointsCampaign> AddPoints(string numPoints, string campaignId, string customerId);

        /// <summary>
        /// Richiede l'aggiunta di punti del cliente associato ad un'azione salvata in precedenza su Krake
        /// </summary>
        /// <param name="action">codice dell'azione </param>
        /// <param name="customerId">identificativo della cliente a cui aggiungere i punti </param>
        /// <returns>APIResult con incapsulato id campagna, id cliente e il numero di punti aggiornato</returns>  
        APIResult<CardPointsCampaign> AddPointsFromAction(string action, string customerId);

        /// <summary>
        /// Richiede la donazione di un premio di una certa campagna all'utente autenticato in Orchard
        /// con conseguente decremento dei punti.
        /// </summary>
        /// <param name="rewardId">identificativo del premio da donare</param>
        /// <param name="campaignId">identificativo della campagna su cui è presente il premio da ritirare </param>
        /// <param name="customerId">identificativo della cliente a cui donare il premio </param>
        /// <returns>APIResult con incapsulato il premio donato</returns>
        APIResult<FidelityReward> GiveReward(string rewardId, string campaignId, string customerId);

        /// <summary>
        /// Richiede la lista di tutte le campagne.
        /// </summary>
        /// <returns>APIResult con incapsulato la lista di tutte le campagne, associate al merchant, in cui almeno l'Id è settato</returns>
        APIResult<IEnumerable<FidelityCampaign>> GetCampaignList();

        /// <summary>
        /// Richiede la lista di tutte le azioni che generano punteggio in una determinata campagna.
        /// </summary>
        /// <returns>APIResult con incapsulato la lista di tutte le azioniincampagna</returns>
        APIResult<IEnumerable<ActionInCampaignRecord>> GetActions();

        /// <summary>
        /// Richiede tutti i dettagli della campagna impostata come default
        /// </summary>
        /// <returns>APIResult con incapsulato il FidelityCampaign con tutti i dati reperiti dal provider</returns>
        APIResult<FidelityCampaign> GetCampaignData();

        /// <summary>
        /// Richiede la donazione di un premio della campagna di default all'utente autenticato in Orchard
        /// con conseguente decremento dei punti.
        /// </summary>
        /// <param name="rewardId">identificativo del premio da donare</param>
        /// <param name="customerId">identificativo della cliente a cui donare il premio </param>
        /// <returns>APIResult con incapsulato il premio donato</returns>
        APIResult<FidelityReward> GiveReward(string rewardId, string custormerId);

        /// <summary>
        /// Richiede l'aggiunta di punti del cliente autenticato in Orchard sulla campagna di defauls.
        /// </summary>
        /// <param name="amount">numero di punti da aumentare </param>
        /// <param name="customerId">identificativo della cliente a cui aggiungere i punti </param>
        /// <returns>APIResult con incapsulato id campagna, id cliente e il numero di punti aggiornato</returns>  
        APIResult<CardPointsCampaign> AddPoints(string amount, string customerId);

        /// <summary>
        /// Richiede di registrare dell'utente loggato in Orchard sul provider di fidelity, aggiungedolo ad una determinata campagna
        /// </summary>
        /// <returns>APIResult con incapsulato la lista di tutte le azioniincampagna</returns>
        APIResult<FidelityCustomer> CreateFidelityAccountFromCookie(string campagnId);

        string GetProviderName();
    }

}
