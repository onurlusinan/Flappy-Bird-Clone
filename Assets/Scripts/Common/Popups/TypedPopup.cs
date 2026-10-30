using System;

namespace Common.Popups
{
    // Concrete popups derive from this so the catalog can match them to their data type without a key.
    public abstract class TypedPopup<TData> : Popup where TData : PopupData
    {
        public override Type DataType => typeof(TData);

        protected TData TypedData => (TData)Data;
    }
}
