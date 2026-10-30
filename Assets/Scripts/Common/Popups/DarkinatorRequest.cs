using System;
using Common.Configs;

namespace Common.Popups
{
    // Doubles as the handle returned by DarkinatorService.Show; a null OnClicked makes the overlay inert.
    public sealed class DarkinatorRequest
    {
        public DarkinatorConfig Config { get; set; }
        public Action OnClicked { get; set; }
        public int SortingOrder { get; set; }
    }
}
