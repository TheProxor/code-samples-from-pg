using System;
using System.Collections.Generic;
using System.Linq;
using Drawmasters.Proposal;
using UnityEngine;
using Object = System.Object;


namespace Drawmasters.Ui
{
    public class UiRewardLayout
    {
        #region Helper
        
        [Serializable]
        public class Data
        {
            public bool isShowSkin = default;
            public int rewardsCount = default;
            public Animator animator = default;
            public List<AnchorData> anchors = default;
        }
        
        [Serializable]
        public class AnchorData
        {
            public RectTransform root = default;

            [SerializeField] private List<MultipleRewardType> availableTypes = default;
           
            public bool CanContainType(MultipleRewardType type) =>
                availableTypes.Contains(type);

            public int AvailableTypesCount =>
                availableTypes.Count;
        }
        
        #endregion
        
        
        
        #region Fields

        private readonly List<Data> data;
        private readonly RewardData[] rewards;

        private Data currentData;

        private HashSet<RectTransform> busyAnchors;

        private int sortingOrder;

        private bool isNeedShowSkin;
        #endregion



        #region Properties
        
        public List<UiRewardItem> RewardItems { get; private set; }

        #endregion



        #region Ctor

        public UiRewardLayout(List<Data> _data, RewardData[] _rewards)
        {
            data = _data;
            rewards = _rewards;

            isNeedShowSkin = _rewards.Any(x => x.IsSkinReward());
        }

        #endregion


        #region Public methods

        public void PermormRewardsLayout(int _sortingOrder, bool isAnimated = true)
        {
            busyAnchors = new HashSet<RectTransform>();

            sortingOrder = _sortingOrder;
            
            FindData();
            FillItems();

            currentData.animator.enabled = isAnimated;
        }


        public void Clear()
        {
            if (RewardItems == null)
            {
                return;
            }

            foreach (var item in RewardItems)
            {
                UnityEngine.Object.Destroy(item.gameObject);
            }
            RewardItems.Clear();
        }
        
        
        public void DeinitializeItems()
        {
            if (RewardItems == null)
            {
                return;
            }

            foreach (var item in RewardItems)
            {
                item.DeinitializeUiRewardItem();
            }
        }
        
        #endregion



        #region Private methods

        private void FindData()
        {
            int rewardsCount = rewards.Length;

            currentData = data.Find(i => i.rewardsCount == rewardsCount);
            if (currentData == null)
            {
                CustomDebug.Log("Not implemented logic. Rewards count: " + rewardsCount);
            }
        }


        private void FillItems()
        {
            RewardItems = new List<UiRewardItem>();   
            
            foreach (var i in rewards)
            {
                MultipleRewardType type = ConvertRewardType(i);

                Transform root = FindRootTransform(type);

                if (root == null)
                {
                    continue;
                }
                
                UiRewardItem item = Content.Storage.CreateRewardItem(type, root);
                
                item.InitializeUiRewardItem(i, sortingOrder);
                
                RewardItems.Add(item);
            }
        }


        private Transform FindRootTransform(MultipleRewardType type)
        {
            AnchorData data = currentData.anchors.OrderBy(e => e.AvailableTypesCount)
                                                 .ToList()
                                                 .Find(i => i.CanContainType(type) && 
                                                            !busyAnchors.Contains(i.root));

            if (data == null)
            {
                CustomDebug.Log("Cannot find free anchor. For type: " + type);
                return null;
            }
            else
            {
                busyAnchors.Add(data.root);
            }

            return data.root;
        }



        protected MultipleRewardType ConvertRewardType(RewardData data)
        {
            if (data == null)
            {
                CustomDebug.Log($"RewardData is null in {nameof(UiRewardElement)}. Returned default MultipleRewardType");
                return MultipleRewardType.None;
            }

            if (data.Type == RewardType.Currency)
            {
                return isNeedShowSkin ? MultipleRewardType.CurrencySmallIcon : MultipleRewardType.Currency;
            }
            else if (data.Type == RewardType.ShooterSkin)
            {
                return MultipleRewardType.ShooterSkin;
            }
            else if (data.Type == RewardType.PetSkin)
            {
                return MultipleRewardType.Pet;
            }
            else if (data.Type == RewardType.WeaponSkin)
            {
                return MultipleRewardType.WeaponSkin;
            }
            
            CustomDebug.Log("Not implemented yet. For type: " + data.GetType().Name);

            return MultipleRewardType.None;
        }

        #endregion
    }
}