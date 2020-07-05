using AdvertServiceDomain.Models;
using ApiGateway.Models;
using System.Collections.Generic;

namespace ApiGateway.Auth
{
    /// <summary>
    /// This is the class that handles moderation, see implementation for detail
    /// </summary>
    public interface IStateProvider
    {
        /// <summary>
        /// Changes State and SubState. The StateId and SubStateId is not useful, see notes on GeneralAdvert. See implementation for detail
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="currentState"></param>
        /// <param name="currentSubState"></param>
        /// <param name="requestedState"></param>
        /// <param name="package"></param>
        /// <param name="adType"></param>
        /// <param name="affiliateLink"></param>
        /// <param name="accountSubState"></param>
        /// <param name="isEdit"></param>
        /// <param name="firstTimeAdvertiser"></param>
        /// <param name="category"></param>
        /// <param name="subCategory"></param>
        /// <param name="subSubCategory"></param>
        /// <param name="intention"></param>
        /// <param name="source"></param>
        /// <param name="promotions"></param>
        /// <returns></returns>
        ChangeStateResult ChangeState(
            ApiScopeResult scope,
            string currentState,
            string currentSubState,
            string requestedState,
            string package,
            string adType,
            string affiliateLink,
            string accountSubState,
            bool isEdit,
            bool firstTimeAdvertiser,
            string category,
            string subCategory,
            string subSubCategory,
            string intention,
            string source,
            IEnumerable<AdPromotion> promotions);

        /// <summary>
        /// Checks for changes and delegates to ChangeState
        /// </summary>
        ChangeStateResult ChangeStateAdvert<T>(
            ApiScopeResult scope,
            string state,
            string subState,
            string package,
            string adType,
            string affiliateLink,
            string accountSubState,
            string source,
            T existing,
            T changed,
            bool firstTimeAdvertiser) where T : BaseAdvert;
    }
}
