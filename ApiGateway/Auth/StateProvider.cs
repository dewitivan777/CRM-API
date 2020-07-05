using AdvertServiceDomain.Models;
using ApiGateway.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiGateway.Auth
{
    /// <summary>
    /// State change provider
    /// </summary>
    public class StateProvider : IStateProvider
    {
        /// <summary>
        /// Tuple1: Current substate and requested state
        /// Tuple2: New state, new substate
        /// </summary>
        public static Dictionary<Tuple<string, string>, Tuple<string, string>> UserNoEditMap
            = new Dictionary<Tuple<string, string>, Tuple<string, string>>();

        /// <summary>
        /// Tuple1: Current substate and requested state
        /// Tuple2: New state, new substate
        /// </summary>
        public static Dictionary<Tuple<string, string>, Tuple<string, string>> UserEditMap
            = new Dictionary<Tuple<string, string>, Tuple<string, string>>();

        /// <summary>
        /// Key: Requested State
        /// Tuple: New state, new substate
        /// </summary>
        public static Dictionary<string, Tuple<string, string>> ModerationMap
            = new Dictionary<string, Tuple<string, string>>();

        /// <summary>
        /// Key: Requested State
        /// Tuple: New state, new substate
        /// </summary>
        public static Dictionary<string, Tuple<string, string>> AffiliateMap
            = new Dictionary<string, Tuple<string, string>>();

        public static ChangeStateResult UnknownState
            = new ChangeStateResult("Active", "PostModeration", true, "Unknown State", true, true);
        public static ChangeStateResult UnknownStatePremium
            = new ChangeStateResult("Active", "PostModeration", true, "Unknown State", false, true);
        public static ChangeStateResult AccountBanState
            = new ChangeStateResult("Rejected", "AccountBan", true, "Rule 4: Auto ban if account is banned");

        public static List<string> PremodCategories = new List<string>()
        {
            "Pets",
            "Holiday Accommodation",
            "Livestock",
            "Job Seekers",
            "Casual Work",
            "Caravans",
            "Campers and Motorhomes",
            "Uncategorized",
            "Wendy Houses and Log Cabins",
            "COVID-19 Products and Services"
        };

        static StateProvider()
        {
            //Notes for after:
            //Add loop to check all possible state combinations have been entered
            //Add tests for all the cases

            #region Affiliate Map
            AffiliateMap["Active"] = new Tuple<string, string>("Active", "Moderated");
            AffiliateMap["Pending"] = new Tuple<string, string>("Pending", "PreModeration");
            AffiliateMap["Inactive"] = new Tuple<string, string>("Inactive", "Edited");
            AffiliateMap["Archive"] = new Tuple<string, string>("Archived", "Deleted");
            AffiliateMap["Archived"] = new Tuple<string, string>("Archived", "Deleted");
            AffiliateMap["Rejected"] = new Tuple<string, string>("Rejected", "AdminReject");
            #endregion

            #region Moderation Map
            ModerationMap[""] = new Tuple<string, string>("Active", "Moderated");
            ModerationMap["Active"] = new Tuple<string, string>("Active", "Moderated");
            ModerationMap["Pending"] = new Tuple<string, string>("Pending", "PreModeration");
            ModerationMap["Inactive"] = new Tuple<string, string>("Inactive", "Edited");
            ModerationMap["Archive"] = new Tuple<string, string>("Archived", "AdminArchived");
            ModerationMap["Archived"] = new Tuple<string, string>("Archived", "AdminArchived");
            ModerationMap["Rejected"] = new Tuple<string, string>("Rejected", "AdminReject");

            ModerationMap["InactiveUnpaid"] = new Tuple<string, string>("Inactive", "InactiveUnpaid");
            ModerationMap["ChangeRequest"] = new Tuple<string, string>("Pending", "ChangeRequest");
            ModerationMap["VerifyContactDetails"] = new Tuple<string, string>("Pending", "VerifyContactDetails");
            ModerationMap["AdminReject"] = new Tuple<string, string>("Rejected", "AdminReject");
            ModerationMap["AccountBan"] = new Tuple<string, string>("Rejected", "AccountBan");
            ModerationMap["AdminArchived"] = new Tuple<string, string>("Archived", "AdminArchived");
            ModerationMap["Sold"] = new Tuple<string, string>("Archived", "Sold");
            ModerationMap["Expired"] = new Tuple<string, string>("Archived", "Expired");
            #endregion

            #region User No Edit Map
            //No State
            UserNoEditMap.Add(new Tuple<string, string>("", ""), new Tuple<string, string>("Active", "PostModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("", "Pending"), new Tuple<string, string>("Pending", "PreModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserNoEditMap.Add(new Tuple<string, string>("", "Archived"), new Tuple<string, string>("Archived", "Deleted"));

            //Active            
            UserNoEditMap.Add(new Tuple<string, string>("Moderated", "Inactive"), new Tuple<string, string>("Inactive", "Deactivated"));
            UserNoEditMap.Add(new Tuple<string, string>("PostModeration", "Inactive"), new Tuple<string, string>("Inactive", "InactivePostModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("ActiveUnpaid", "Inactive"), new Tuple<string, string>("Inactive", "InactiveUnpaid"));
            UserNoEditMap.Add(new Tuple<string, string>("ActiveHoliday", "Inactive"), new Tuple<string, string>("Inactive", "Deactivated"));

            UserNoEditMap.Add(new Tuple<string, string>("Moderated", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("PostModeration", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("ActiveUnpaid", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("ActiveHoliday", "Archived"), new Tuple<string, string>("Archived", "Deleted"));

            UserNoEditMap.Add(new Tuple<string, string>("Moderated", "Active"), new Tuple<string, string>("Active", "Moderated"));
            UserNoEditMap.Add(new Tuple<string, string>("PostModeration", "Active"), new Tuple<string, string>("Active", "PostModeration"));

            //Pending
            UserNoEditMap.Add(new Tuple<string, string>("PreModeration", "Active"), new Tuple<string, string>("Pending", "PreModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("ChangeRequest", "Active"), new Tuple<string, string>("Pending", "ChangeRequest"));
            UserNoEditMap.Add(new Tuple<string, string>("VerifyContactDetails", "Active"), new Tuple<string, string>("Pending", "PreModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("PendingSpam", "Active"), new Tuple<string, string>("Pending", "PendingSpam"));
            UserNoEditMap.Add(new Tuple<string, string>("PendingReview", "Active"), new Tuple<string, string>("Pending", "PendingReview"));

            UserNoEditMap.Add(new Tuple<string, string>("PreModeration", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserNoEditMap.Add(new Tuple<string, string>("ChangeRequest", "Inactive"), new Tuple<string, string>("Inactive", "InactiveChangeRequested"));
            UserNoEditMap.Add(new Tuple<string, string>("VerifyContactDetails", "Inactive"), new Tuple<string, string>("Inactive", "InactiveVerifyContactDetails"));
            UserNoEditMap.Add(new Tuple<string, string>("PendingSpam", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserNoEditMap.Add(new Tuple<string, string>("PendingReview", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));

            UserNoEditMap.Add(new Tuple<string, string>("PreModeration", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("ChangeRequest", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("VerifyContactDetails", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("PendingSpam", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("PendingReview", "Archived"), new Tuple<string, string>("Archived", "Deleted"));

            //Inactive
            UserNoEditMap.Add(new Tuple<string, string>("Deactivated", "Active"), new Tuple<string, string>("Active", "Moderated"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveUnpaid", "Active"), new Tuple<string, string>("Active", "ActiveUnpaid")); //This one might need some discussion
            UserNoEditMap.Add(new Tuple<string, string>("InactiveEdited", "Active"), new Tuple<string, string>("Pending", "PreModeration"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveChangeRequested", "Active"), new Tuple<string, string>("Pending", "ChangeRequest"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveVerifyContactDetails", "Active"), new Tuple<string, string>("Pending", "VerifyContactDetails"));
            UserNoEditMap.Add(new Tuple<string, string>("InactivePostModeration", "Active"), new Tuple<string, string>("Active", "PostModeration"));

            UserNoEditMap.Add(new Tuple<string, string>("Edited", "Inactive"), new Tuple<string, string>("Inactive", "Edited"));
            UserNoEditMap.Add(new Tuple<string, string>("InactivePostModeration", "Inactive"), new Tuple<string, string>("Inactive", "InactivePostModeration"));

            UserNoEditMap.Add(new Tuple<string, string>("Deactivated", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveUnpaid", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveEdited", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveChangeRequested", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("InactiveVerifyContactDetails", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("InactivePostModeration", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserNoEditMap.Add(new Tuple<string, string>("Edited", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            #endregion

            #region User Edit Map
            //No State
            UserEditMap.Add(new Tuple<string, string>("", ""), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("", "Pending"), new Tuple<string, string>("Pending", "PreModeration"));
            UserEditMap.Add(new Tuple<string, string>("", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("", "Archived"), new Tuple<string, string>("Archived", "Deleted"));

            //Active            
            UserEditMap.Add(new Tuple<string, string>("Moderated", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("PostModeration", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("ActiveUnpaid", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("ActiveHoliday", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));

            UserEditMap.Add(new Tuple<string, string>("Moderated", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("PostModeration", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("ActiveUnpaid", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("ActiveHoliday", "Archived"), new Tuple<string, string>("Archived", "Deleted"));

            UserEditMap.Add(new Tuple<string, string>("Moderated", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("PostModeration", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("ActiveUnpaid", "Active"), new Tuple<string, string>("Pending", "PreModeration")); //Needs discussion
            UserEditMap.Add(new Tuple<string, string>("ActiveHoliday", "Active"), new Tuple<string, string>("Pending", "PreModeration")); //Needs discussion

            //Pending
            UserEditMap.Add(new Tuple<string, string>("PreModeration", "Active"), new Tuple<string, string>("Pending", "PreModeration"));
            UserEditMap.Add(new Tuple<string, string>("ChangeRequest", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("VerifyContactDetails", "Active"), new Tuple<string, string>("Pending", "VerifyContactDetails"));

            UserEditMap.Add(new Tuple<string, string>("PreModeration", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("ChangeRequest", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("VerifyContactDetails", "Inactive"), new Tuple<string, string>("Inactive", "InactiveVerifyContactDetails"));

            UserEditMap.Add(new Tuple<string, string>("PreModeration", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("ChangeRequest", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("VerifyContactDetails", "Archived"), new Tuple<string, string>("Archived", "Deleted"));

            //Inactive
            UserEditMap.Add(new Tuple<string, string>("Deactivated", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("InactiveUnpaid", "Active"), new Tuple<string, string>("Inactive", "InactiveUnpaid"));
            UserEditMap.Add(new Tuple<string, string>("InactiveEdited", "Active"), new Tuple<string, string>("Pending", "PreModeration"));
            UserEditMap.Add(new Tuple<string, string>("InactiveChangeRequested", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("InactiveVerifyContactDetails", "Active"), new Tuple<string, string>("Pending", "VerifyContactDetails"));
            UserEditMap.Add(new Tuple<string, string>("InactivePostModeration", "Active"), new Tuple<string, string>("Active", "PostModeration"));
            UserEditMap.Add(new Tuple<string, string>("Edited", "Active"), new Tuple<string, string>("Pending", "PreModeration"));

            UserEditMap.Add(new Tuple<string, string>("Deactivated", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("InactiveUnpaid", "Inactive"), new Tuple<string, string>("Inactive", "InactiveUnpaid"));
            UserEditMap.Add(new Tuple<string, string>("InactiveEdited", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("InactiveChangeRequested", "Inactive"), new Tuple<string, string>("Inactive", "InactiveEdited"));
            UserEditMap.Add(new Tuple<string, string>("InactiveVerifyContactDetails", "Inactive"), new Tuple<string, string>("Inactive", "VerifyContactDetails"));
            UserEditMap.Add(new Tuple<string, string>("Edited", "Inactive"), new Tuple<string, string>("Inactive", "Edited"));

            UserEditMap.Add(new Tuple<string, string>("Deactivated", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("InactiveUnpaid", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("InactiveEdited", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("InactiveChangeRequested", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("InactiveVerifyContactDetails", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("InactivePostModeration", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            UserEditMap.Add(new Tuple<string, string>("Edited", "Archived"), new Tuple<string, string>("Archived", "Deleted"));
            #endregion
        }

        /// <summary>
        ///  Rule 1: There are 5 main states that determine when and where content is shown: Active, Pending, Inactive, Archived, Rejected<para/>  
        ///  Rule 2: The list of substates is ongoing and determines the next state<para/>  
        ///  Rule 3: Affiliate content from Auto Mart, Truck &amp; Trailer, Agrimag and Job Mail use the AffiliateMap<para/> 
        ///  Rule 3.b: CCAP content use the AffiliateMap<para/> 
        ///  Rule 4: Auto ban if account is banned<para/> 
        ///  Rule 6: Request is made by a moderator<para/> 
        ///  Rule 6.a: If the account is Unpaid and the requested state is active or inactive, move to ActiveUnpaid or InactiveUnpaid<para/> 
        ///  Rule 6.b: The state is change according to the ModerationMap<para/>  
        ///  Rule 7: Promoted Content. The state is set to Active PostModeration
        ///  Rule 10: If the account is unpaid<para/> 
        ///  Rule 10.a: If the content is edited, the state is set to Pending PreModeration<para/> 
        ///  Rule 10.b: Archived ads are archived<para/> 
        ///  Rule 10.c: The ads go to Active Unpaid or Inactive Unpaid states<para/> 
        ///  Rule 11: If the ad is not edited, the state is changed according to UserNoEditMap<para/> 
        ///  Rule 12: If the ad is edited<para/> 
        ///  Rule 12.a: If the user is a first time advertiser, the state is set to Pending PreModeration<para/> 
        ///  Rule 12.b: If the user is premium, set the state to Active PostModeration and skip PreModeration keyword check<para/> 
        ///  Rule 12.c: If the category is premod, the state is set to Pending PreModeration<para/> 
        ///  Rule 12.c.ii: If package is owner and category is cars, the state is set to Pending Premoderation 
        ///  Rule 12.d: If the intention is give away, the state is set to Pending PreModerationg<para/>
        ///  Rule 12.e: The state is changed according to UserEditMap<para/>
        ///  Rule 13: If the ad matches rule 12, the content is checked for spam<para/>
        ///  Rule 13.a: Found more than 9 high level unicode in text<para/>
        ///  Rule 13.b: Found Reject keyword in text<para/>
        ///  Rule 13.c: Found Premod keyword in text<para/>
        ///  Rule 13.d: Found Url in text<para/>
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
        public ChangeStateResult ChangeState(
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
            IEnumerable<AdPromotion> promotions)
        {
            // modified
            UnknownState.Modified = isEdit;
            UnknownStatePremium.Modified = isEdit;
            AccountBanState.Modified = isEdit;

            if (string.IsNullOrEmpty(currentState))
                currentState = "";

            //we assume if no state has been specified, the poster intends for the ad to go active
            if (string.IsNullOrEmpty(requestedState))
                requestedState = "Active";

            //Rule 3
            affiliateLink = (affiliateLink ?? "").ToLower();

            if (affiliateLink.Contains("auto") || affiliateLink.Contains("truck") || affiliateLink.Contains("agri") || affiliateLink.Contains("job") || affiliateLink.Contains("gotproperty"))
            {
                if (!AffiliateMap.TryGetValue(requestedState, out var affiliateState))
                {
                    //All these state should be mapped
                    return UnknownState;
                }
                else
                {
                    return new ChangeStateResult(
                        affiliateState.Item1,
                        affiliateState.Item2,
                        isEdit,
                        "Rule 3: Affiliate content from Auto Mart, Truck &amp; Trailer, Agrimag and Job Mail use the AffiliateMap");
                }
            }

            //Rule 3.b
            if (string.Compare(source, "ccap", StringComparison.OrdinalIgnoreCase) == 0)
            {
                if (!AffiliateMap.TryGetValue(requestedState, out var affiliateState))
                {
                    //All these state should be mapped
                    return UnknownState;
                }
                else
                {
                    return new ChangeStateResult(
                        affiliateState.Item1,
                        affiliateState.Item2,
                        isEdit,
                        "Rule 3b: CCAP content use the AffiliateMap");
                }
            }

            //Rule 4
            if (accountSubState == "AccountBan")
                return AccountBanState;

            var stateKey = new Tuple<string, string>(currentSubState, requestedState);

            //Rule 5 (deprecated since it does not take into account subState)
            //if (!isEdit && requestedState == currentState)
            //{
            //    return new ChangeStateResult(currentState, currentSubState, "Rule 5: If the ad is not edited and no new state is requested, the current state is kept");
            //}

            //Rule 6
            if (scope.IsModerator)
            {
                if (accountSubState == "ActiveUnpaid" || accountSubState == "InactiveUnpaid")
                {
                    if (requestedState == "Active")
                        return new ChangeStateResult(
                            "Active",
                            "ActiveUnpaid",
                            isEdit,
                            "Rule 6.a: If the account is Unpaid and the requested active, move to ActiveUnpaid");
                    else if (requestedState == "Inactive")
                        return new ChangeStateResult(
                            "Inactive",
                            "InactiveUnpaid",
                            isEdit,
                            "Rule 6.a: If the account is Unpaid and the requested state is inactive, move to InactiveUnpaid");
                }

                if (ModerationMap.TryGetValue(requestedState, out var moderatorState))
                {
                    return new ChangeStateResult(
                        moderatorState.Item1,
                        moderatorState.Item2,
                        isEdit,
                        "Rule 6.b: The state is change according to the ModerationMap");
                }
                //Unmapped moderator state, could indicate new font end functionality
                else
                {
                    return UnknownState;
                }
            }
            else
            {
                //Rule 7
                if (promotions != null && promotions.Any(tbl => tbl.EndDate > DateTime.Now && (tbl.AdPromotionType?.ToLower() == "top" || tbl.AdPromotionType?.ToLower() == "homepage")))
                {
                    var recommendedStateChange = new ChangeStateResult(
                        "Active",
                        "PostModeration",
                        isEdit,
                        "Rule 7: Promoted Content",
                        false,
                        true);

                    if (requestedState == "Active")
                    {
                        return recommendedStateChange;
                    }
                    // use no edit for other actions?
                    else if (UserNoEditMap.TryGetValue(stateKey, out var noEditState))
                    {
                        return new ChangeStateResult(
                            noEditState.Item1,
                            noEditState.Item2,
                            isEdit,
                            "Rule 7: Promoted Content",
                            false,
                            true);
                    }

                    return recommendedStateChange;
                }

                //Rule 10
                if (accountSubState == "ActiveUnpaid" || accountSubState == "InactiveUnpaid")
                {
                    //Rule 10.a
                    if (isEdit)
                        return new ChangeStateResult(
                            "Pending",
                            "PreModeration",
                            isEdit,
                            "Rule 10.a: If the content is edited, the state is set to Pending PreModeration");

                    //Rule 10.c
                    if (requestedState == "Active")
                        return new ChangeStateResult(
                            "Active",
                            "ActiveUnpaid",
                            isEdit,
                            "Rule 10.c: The ads go to Active Unpaid");
                    else if (requestedState == "Inactive")
                        return new ChangeStateResult(
                            "Inactive",
                            "InactiveUnpaid",
                            isEdit,
                            "Rule 10.c: The ads go to Inactive Unpaid");
                    else
                    {
                        //Rule 10.b
                        //This rule falls to the edit rule with archived state
                    }
                }

                //Rule 11
                if (!isEdit && currentSubState != "InactiveEdited")
                {
                    if (UserNoEditMap.TryGetValue(stateKey, out var noEditState))
                    {
                        return new ChangeStateResult(
                            noEditState.Item1,
                            noEditState.Item2,
                            isEdit,
                            "Rule 11: If the ad is not edited, the state is changed according to UserNoEditMap",
                            false,
                            true);
                    }
                    //These state should all be mapped. Could indicate an invalid request
                    else
                    {
                        //Defer to edit check when no state is specified
                        //if (package?.ToLower().Contains("premium") == true)
                        //{
                        //    return UnknownStatePremium;
                        //}
                        //else
                        //{
                        //    return UnknownState;
                        //}
                    }
                }

                //Rule 12
                //Rule 12.a
                if (firstTimeAdvertiser)
                {
                    return new ChangeStateResult(
                        "Pending",
                        "PreModeration",
                        isEdit,
                        "Rule 12.a: First time advertiser",
                        true,
                        true);
                }

                //Rule 12.b
                if (package?.ToLower().Contains("premium") == true)
                {
                    return new ChangeStateResult(
                        "Active",
                        "PostModeration",
                        isEdit,
                        "Rule 12.b: Premium User",
                        false,
                        true);
                }

                //Rule 12.c
                if (PremodCategories.Contains(category, StringComparer.OrdinalIgnoreCase)
                    || PremodCategories.Contains(subCategory, StringComparer.OrdinalIgnoreCase)
                    || PremodCategories.Contains(subSubCategory, StringComparer.OrdinalIgnoreCase))
                {
                    return new ChangeStateResult(
                        "Pending",
                        "PreModeration",
                        isEdit,
                        "Rule 12.c: Premod Category",
                        true,
                        true);
                }

                //Rule 12.c.ii Owner Motoring
                if (category?.ToLower() == "cars" && package?.ToLower() == "owner")
                {
                    return new ChangeStateResult(
                        "Pending",
                        "PreModeration",
                        isEdit,
                        "Rule 12.c.ii: Owner Cars",
                        true,
                        true);
                }

                //Rule 12.d
                if (intention == "Give Away")
                {
                    return new ChangeStateResult(
                        "Pending",
                        "PreModeration",
                        isEdit,
                        "Rule 12.d: Give Away",
                        true,
                        true);
                }

                if (UserEditMap.TryGetValue(stateKey, out var editState))
                {
                    //Rule 12.e
                    return new ChangeStateResult(
                        editState.Item1,
                        editState.Item2,
                        isEdit,
                        "Rule 12.e: If the ad is edited, the state is changed according to the UserEditMap",
                        true,
                        true);
                }
                //These state should all be mapped. Could indicate an invalid request
                else
                {
                    //This code is unreachable, see 12.b
                    if (package?.ToLower().Contains("premium") == true)
                    {
                        return UnknownStatePremium;
                    }
                    else
                    {
                        return UnknownState;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if the advert has been changed and defers to ChangeState
        /// </summary>
        /// <param name="scope"></param>
        /// <param name="state"></param>
        /// <param name="subState"></param>
        /// <param name="package"></param>
        /// <param name="adType"></param>
        /// <param name="affiliateLink"></param>
        /// <param name="accountSubState"></param>
        /// <param name="source"></param>
        /// <param name="existing"></param>
        /// <param name="changed"></param>
        /// <param name="firstTimeAdvertiser"></param>
        /// <returns></returns>
        public ChangeStateResult ChangeStateAdvert<T>(
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
            bool firstTimeAdvertiser) where T : BaseAdvert
        {
            bool isEdit = CheckChange(existing, changed);

            return ChangeState(
                scope,
                state,
                subState,
                changed.State,
                package,
                adType,
                affiliateLink,
                accountSubState,
                isEdit,
                firstTimeAdvertiser,
                changed.Category,
                changed.SubCategory,
                changed.SubSubCategory,
                changed.Intention,
                source,
                changed.AdPromotions);
        }

        /// <summary>
        /// Only changes to certain fields should be considered when sending ads for remoderation
        /// </summary>
        /// <param name="existing"></param>
        /// <param name="changed"></param>
        /// <returns></returns>
        public bool CheckChange<T>(T existing, T changed) where T : BaseAdvert
        {
            var edited = existing.Equals(changed);

            return !edited;
        }
    }
}
