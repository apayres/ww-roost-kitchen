namespace Roost.Kitchen.App.Models
{
    public class Ingredient
    {
        public string Upc { set; get; }

        public string ItemName { get; set; }

        public string ItemDescription { get; set; }

        public double Ratio { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; }
    }
}
