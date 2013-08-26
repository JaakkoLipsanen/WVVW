using System.Collections.ObjectModel;
using System.Globalization;
using Flai;
using Flai.Graphics;
using Microsoft.Xna.Framework;
using WVVW.Model;

namespace WVVW.View
{
    public class TimeBonusRenderer : FlaiRenderer
    {
        private readonly ReadOnlyCollection<TimeBonus> _timeBonuses;
        public TimeBonusRenderer(ReadOnlyCollection<TimeBonus> timeBonuses)
        {
            _timeBonuses = timeBonuses;
        }

        protected override void UpdateInner(UpdateContext updateContext)
        {
        }

        protected override void DrawInner(GraphicsContext graphicsContext)
        {
            foreach (TimeBonus timeBonus in _timeBonuses)
            {
                if (!timeBonus.IsCollected)
                {
                    TileRenderer.DrawTile(graphicsContext, timeBonus.Position, timeBonus.TileType);
                }
                else
                {
                    const string FontName = "Minecraftia18";
                    float alpha = timeBonus.Alpha * timeBonus.Alpha;
                    graphicsContext.SpriteBatch.DrawStringCentered(graphicsContext.FontContainer[FontName], "+" + timeBonus.Time.ToString("0.00", CultureInfo.InvariantCulture), timeBonus.Position - Vector2i.UnitY * (12 + 32 * (1 - alpha)), Color.Teal);
                }
            }
        }
    }
}
