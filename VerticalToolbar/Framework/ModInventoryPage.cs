using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace SB_VerticalToolMenu.Framework
{
    internal class ModInventoryPage : InventoryPage
    {
        private readonly VerticalToolBar verticalToolBar;

        public ModInventoryPage(int x, int y, int width, int height)
            : base(x, y, width, height)
        {
            verticalToolBar = new VerticalToolBar(
                xPositionOnScreen - spaceToClearSideBorder - borderWidth * 2,
                yPositionOnScreen + spaceToClearTopBorder - borderWidth / 2 + 4,
                VerticalToolBar.NUM_BUTTONS,
                true);
        }

        public override void performHoverAction(int x, int y)
        {
            verticalToolBar.performHoverAction(x, y);
            base.performHoverAction(x, y);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            var heldItem = Game1.player.CursorSlotItem;
            for (var i = Game1.player.MaxItems; i < Farmer.maxInventorySpace; i++)
                if (Game1.player.Items[i] != null)
                {
                    Game1.player.CursorSlotItem = Game1.player.Items[i];
                    Game1.player.Items[i] = null;
                }

            foreach (var button in verticalToolBar.buttons)
                if (button.containsPoint(x, y))
                {
                    if (heldItem != null)
                    {
                        if (Game1.player.Items[Convert.ToInt32(button.name)] == null ||
                            Game1.player.Items[Convert.ToInt32(button.name)].canStackWith(heldItem))
                        {
                            if (Game1.player.CurrentToolIndex == Convert.ToInt32(button.name))
                                heldItem.actionWhenBeingHeld(Game1.player);
                            Utility.addItemToInventory(heldItem, Convert.ToInt32(button.name), Game1.player.Items);
                            Game1.player.CursorSlotItem = null;
                            Game1.playSound("stoneStep");
                            return;
                        }

                        if (Game1.player.Items[Convert.ToInt32(button.name)] != null)
                        {
                            var swapItem = Game1.player.CursorSlotItem;
                            Game1.player.CursorSlotItem = Game1.player.Items[Convert.ToInt32(button.name)];
                            Utility.addItemToInventory(swapItem, Convert.ToInt32(button.name), Game1.player.Items);
                            return;
                        }
                    }

                    if (Game1.player.Items[Convert.ToInt32(button.name)] != null)
                    {
                        Game1.player.CursorSlotItem = Game1.player.Items[Convert.ToInt32(button.name)];
                        Utility.removeItemFromInventory(Convert.ToInt32(button.name), Game1.player.Items);
                        return;
                    }
                }

            if (organizeButton.containsPoint(x, y))
            {
                var items = Game1.player.Items.ToList();
                items.Sort(0, 36, null);
                items.Reverse(0, 36);
                Game1.player.Items = items;
                Game1.playSound("Ship");
                return;
            }

            base.receiveLeftClick(x, y);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            if (verticalToolBar.isWithinBounds(x, y))
            {
                var heldItem = Game1.player.CursorSlotItem;
                Game1.player.CursorSlotItem = verticalToolBar.rightClick(x, y, heldItem, playSound);
                return;
            }

            base.receiveRightClick(x, y, playSound);
        }

        public override void draw(SpriteBatch b)
        {
            for (var index = 0; index < VerticalToolBar.NUM_BUTTONS; ++index)
                verticalToolBar.buttons[index].bounds = new Rectangle(
                    //TODO: Use more reliable coordinates
                    verticalToolBar.xPositionOnScreen,
                    verticalToolBar.yPositionOnScreen + index * Game1.tileSize,
                    Game1.tileSize,
                    Game1.tileSize);
            base.draw(b);
            verticalToolBar.draw(b);
        }
    }
}