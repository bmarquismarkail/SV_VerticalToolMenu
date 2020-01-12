using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using SObject = StardewValley.Object;

namespace SB_VerticalToolMenu.Framework
{
    internal class VerticalToolBar : IClickableMenu
    {
        public static int NUM_BUTTONS = 5;
        public List<ClickableComponent> buttons = new List<ClickableComponent>();
        public bool forceDraw;
        private Item hoverItem;
        private string hoverTitle = "";
        public int numToolsinToolbar;
        public Rectangle toolbarTextSource = new Rectangle(0, 256, 60, 60);
        private float transparency = 1f;

        public VerticalToolBar(int x, int y, int numButtons = 5, bool forceDraw = false)
            : base(x, y,
                Game1.tileSize * 3 / 2,
                Game1.tileSize * numButtons + Game1.tileSize / 2)
        {
            NUM_BUTTONS = numButtons;
            this.forceDraw = forceDraw;
            for (var count = Game1.player.Items.Count; count < 36 + NUM_BUTTONS; count++) Game1.player.Items.Add(null);

            for (var index = 0; index < NUM_BUTTONS; ++index)
                buttons.Add(
                    new ClickableComponent(
                        new Rectangle(
                            Game1.viewport.Width / 2 - Game1.tileSize * 15 / 2 - Game1.pixelZoom * 4 + 16,
                            yPositionOnScreen + index * Game1.tileSize,
                            Game1.tileSize,
                            Game1.tileSize),
                        string.Concat(index + 36)));
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (Game1.player.UsingTool)
                return;
            foreach (var button in buttons)
                if (button.containsPoint(x, y))
                {
                    Game1.player.CurrentToolIndex = Convert.ToInt32(button.name);
                    if (Game1.player.ActiveObject != null)
                    {
                        Game1.player.showCarrying();
                        Game1.playSound("pickUpItem");
                        break;
                    }

                    Game1.player.showNotCarrying();
                    Game1.playSound("stoneStep");
                    break;
                }
        }

        public Item rightClick(int x, int y, Item toAddTo, bool playSound = true)
        {
            foreach (var button in buttons)
            {
                var int32 = Convert.ToInt32(button.name);
                var x1 = x;
                var y1 = y;
                if (button.containsPoint(x1, y1) && Game1.player.Items[int32] != null)
                {
                    if (Game1.player.Items[int32] is Tool && (toAddTo == null || toAddTo is SObject) &&
                        (Game1.player.Items[int32] as Tool).canThisBeAttached((SObject) toAddTo))
                        return (Game1.player.Items[int32] as Tool).attach((SObject) toAddTo);
                    if (toAddTo == null)
                    {
                        if (Game1.player.Items[int32].maximumStackSize() != -1)
                        {
                            if (int32 == Game1.player.CurrentToolIndex && Game1.player.Items[int32] != null &&
                                Game1.player.Items[int32].Stack == 1)
                                Game1.player.Items[int32].actionWhenStopBeingHeld(Game1.player);
                            var one = Game1.player.Items[int32].getOne();
                            if (Game1.player.Items[int32].Stack > 1)
                                if (Game1.isOneOfTheseKeysDown(Game1.oldKBState,
                                    new[] {new InputButton(Keys.LeftShift)}))
                                {
                                    one.Stack = (int) Math.Ceiling(Game1.player.Items[int32].Stack / 2.0);
                                    Game1.player.Items[int32].Stack = Game1.player.Items[int32].Stack / 2;
                                    goto label_15;
                                }

                            if (Game1.player.Items[int32].Stack == 1)
                                Game1.player.Items[int32] = null;
                            else
                                --Game1.player.Items[int32].Stack;
                            label_15:
                            if (Game1.player.Items[int32] != null && Game1.player.Items[int32].Stack <= 0)
                                Game1.player.Items[int32] = null;
                            if (playSound)
                                Game1.playSound("dwop");
                            return one;
                        }
                    }
                    else if (Game1.player.Items[int32].canStackWith(toAddTo) &&
                             toAddTo.Stack < toAddTo.maximumStackSize())
                    {
                        if (Game1.isOneOfTheseKeysDown(Game1.oldKBState, new[] {new InputButton(Keys.LeftShift)}))
                        {
                            toAddTo.Stack += (int) Math.Ceiling(Game1.player.Items[int32].Stack / 2.0);
                            Game1.player.Items[int32].Stack = Game1.player.Items[int32].Stack / 2;
                        }
                        else
                        {
                            ++toAddTo.Stack;
                            --Game1.player.Items[int32].Stack;
                        }

                        if (playSound)
                            Game1.playSound("dwop");
                        if (Game1.player.Items[int32].Stack <= 0)
                        {
                            if (int32 == Game1.player.CurrentToolIndex)
                                Game1.player.Items[int32].actionWhenStopBeingHeld(Game1.player);
                            Game1.player.Items[int32] = null;
                        }

                        return toAddTo;
                    }
                }
            }

            return toAddTo;
        }

        public override void performHoverAction(int x, int y)
        {
            hoverItem = null;
            foreach (var button in buttons)
                if (button.containsPoint(x, y))
                {
                    var int32 = Convert.ToInt32(button.name);
                    if (int32 < Game1.player.Items.Count && Game1.player.Items[int32] != null)
                    {
                        button.scale = Math.Min(button.scale + 0.05f, 1.1f);
                        hoverTitle = Game1.player.Items[int32].Name;
                        hoverItem = Game1.player.Items[int32];
                    }
                }
                else
                {
                    button.scale = Math.Max(button.scale - 0.025f, 1f);
                }
        }

        public void shifted(bool right)
        {
            if (right)
                for (var index = 0; index < buttons.Count; ++index)
                    buttons[index].scale = (float) (1.0 + index * 0.0299999993294477);
            else
                for (var index = buttons.Count - 1; index >= 0; --index)
                    buttons[index].scale = (float) (1.0 + (11 - index) * 0.0299999993294477);
        }

        public override void update(GameTime time)
        {
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            for (var index = 0; index < NUM_BUTTONS; ++index)
                buttons[index].bounds = new Rectangle(
                    //TODO: Use more reliable coordinates
                    Game1.activeClickableMenu is GameMenu
                        ? xPositionOnScreen
                        : Game1.viewport.Width / 2 - Game1.tileSize * 15 / 2 - Game1.pixelZoom * 4,
                    yPositionOnScreen + index * Game1.tileSize,
                    Game1.tileSize,
                    Game1.tileSize);
        }

        public override bool isWithinBounds(int x, int y)
        {
            return new Rectangle(
                buttons.First().bounds.X,
                buttons.First().bounds.Y,
                Game1.tileSize,
                buttons.Last().bounds.Y - buttons.First().bounds.Y + Game1.tileSize
            ).Contains(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            //Checks if the player is on any other menu before drawing the tooltip
            if (Game1.activeClickableMenu != null && !forceDraw)
                return;
            //Checks and draws the buttons
            if (!forceDraw)
            {
                var positionOnScreen1 = yPositionOnScreen;
                if (Game1.options.pinToolbarToggle)
                {
                    yPositionOnScreen = Game1.viewport.Height - getInitialHeight();
                    transparency = Math.Min(1f, transparency + 0.075f);
                    if (Game1.GlobalToLocal(Game1.viewport,
                                new Vector2(Game1.player.GetBoundingBox().Center.X,
                                    Game1.player.GetBoundingBox().Center.Y))
                            .Y > (double) (Game1.viewport.Height - Game1.tileSize * 3))
                        transparency = Math.Max(0.33f, transparency - 0.15f);
                }
                else
                {
                    yPositionOnScreen =
                        (double) Game1.GlobalToLocal(Game1.viewport,
                                new Vector2(Game1.player.GetBoundingBox().Center.X,
                                    Game1.player.GetBoundingBox().Center.Y))
                            .Y > (double) (Game1.viewport.Height / 2 + Game1.tileSize)
                            ? Game1.tileSize / 8
                            : Game1.viewport.Height - getInitialHeight() - Game1.tileSize / 8;
                }

                var positionOnScreen2 = yPositionOnScreen;
                if (positionOnScreen1 != positionOnScreen2)
                    for (var index = 0; index < NUM_BUTTONS; ++index)
                        buttons[index].bounds.Y = yPositionOnScreen + index * Game1.tileSize;
            }

            //Draws the background texture. 
            drawTextureBox(
                b,
                Game1.menuTexture,
                toolbarTextSource,
                //TODO: Use more reliable coordinates
                Game1.activeClickableMenu is GameMenu
                    ? xPositionOnScreen
                    : Game1.viewport.Width / 2 - Game1.tileSize * 15 / 2 - Game1.pixelZoom * 4,
                yPositionOnScreen,
                Game1.tileSize * 3 / 2,
                Game1.tileSize * NUM_BUTTONS + Game1.tileSize / 2,
                Color.White * transparency, 1f, false);
            var toolBarIndex = 0;
            for (var index = 0; index < NUM_BUTTONS; ++index)
            {
                buttons[index].scale = Math.Max(1f, buttons[index].scale - 0.025f);
                var location = new Vector2(
                    //TODO: Use more reliable coordinates
                    (Game1.activeClickableMenu is GameMenu
                        ? xPositionOnScreen
                        : Game1.viewport.Width / 2 - Game1.tileSize * 15 / 2 - Game1.pixelZoom * 4) + 16,
                    yPositionOnScreen + index * Game1.tileSize + 16);
                b.Draw(Game1.menuTexture, location,
                    Game1.getSourceRectForStandardTileSheet(Game1.menuTexture,
                        Game1.player.CurrentToolIndex == index + 36 ? 56 : 10), Color.White * transparency);
                // Need to customize it for toolset //string text = index == 9 ? "0" : (index == 10 ? "-" : (index == 11 ? "=" : string.Concat((object)(index + 1))));
                //b.DrawString(Game1.tinyFont, text, position + new Vector2(4f, -8f), Color.DimGray * this.transparency);
                if (Game1.player.Items.Count > index + 36 && Game1.player.Items.ElementAt(index + 36) != null)
                {
                    Game1.player.Items[index + 36].drawInMenu(b, location,
                        Game1.player.CurrentToolIndex == index + 36 ? 0.9f : buttons.ElementAt(index).scale * 0.8f,
                        transparency, 0.88f);
                    toolBarIndex++;
                }
            }

            if (toolBarIndex != numToolsinToolbar)
                numToolsinToolbar = toolBarIndex;

            //If an item is hovered, shows its tooltip.
            if (hoverItem == null)
                return;
            drawToolTip(b, hoverItem.getDescription(), hoverItem.Name, hoverItem);
            hoverItem = null;
        }

        public static int getInitialWidth()
        {
            return Game1.tileSize * 3 / 2;
        }

        public static int getInitialHeight()
        {
            return Game1.tileSize * NUM_BUTTONS + Game1.tileSize / 2;
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
    }
}