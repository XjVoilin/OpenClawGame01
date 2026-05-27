#!/usr/bin/env python3
"""Extract individual UI sprites from Sprout Lands UI Pack spritesheets."""

from PIL import Image
import os
import shutil

ARTS_ROOT = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "Assets", "Game", "Arts", "SproutLands",
    "Sprout Lands - UI Pack - Premium pack", "UI Sprites"
)
OUT_DIR = os.path.join(
    os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
    "Assets", "Game", "Res", "Sprites", "UI"
)

os.makedirs(OUT_DIR, exist_ok=True)


def save(img, name):
    path = os.path.join(OUT_DIR, f"{name}.png")
    img.save(path)
    print(f"  -> {name}.png ({img.size[0]}x{img.size[1]})")


def find_cells(img):
    """Find non-transparent cell bounds by scanning for gap rows/cols."""
    px = img.load()
    w, h = img.size

    def row_empty(r):
        return all(px[c, r][3] == 0 for c in range(w))

    def col_empty(c):
        return all(px[c, r][3] == 0 for r in range(h))

    row_ranges = []
    in_content = False
    start = 0
    for r in range(h):
        if not row_empty(r):
            if not in_content:
                start = r
                in_content = True
        else:
            if in_content:
                row_ranges.append((start, r))
                in_content = False
    if in_content:
        row_ranges.append((start, h))

    col_ranges = []
    in_content = False
    for c in range(w):
        if not col_empty(c):
            if not in_content:
                start = c
                in_content = True
        else:
            if in_content:
                col_ranges.append((start, c))
                in_content = False
    if in_content:
        col_ranges.append((start, w))

    return col_ranges, row_ranges


def extract_square_buttons():
    """Extract buttons from Square Buttons 26x26.png -> 8 buttons."""
    print("\n[Square Buttons 26x26]")
    path = os.path.join(ARTS_ROOT, "buttons", "square", "Square Buttons 26x26.png")
    img = Image.open(path).convert("RGBA")
    cols, rows = find_cells(img)
    print(f"  Grid: {len(cols)} cols x {len(rows)} rows")

    names = [
        ["SL_UI_Btn_Lightest", "SL_UI_Btn_Lightest_Dark"],
        ["SL_UI_Btn_Light", "SL_UI_Btn_Light_Dark"],
        ["SL_UI_Btn_Medium", "SL_UI_Btn_Medium_Dark"],
        ["SL_UI_Btn_Dark", "SL_UI_Btn_Dark_Dark"],
    ]
    for ri, (ry0, ry1) in enumerate(rows):
        for ci, (cx0, cx1) in enumerate(cols):
            cell = img.crop((cx0, ry0, cx1, ry1))
            if ri < len(names) and ci < len(names[ri]):
                save(cell, names[ri][ci])


def extract_small_square_buttons():
    """Extract from Small Square Buttons.png."""
    print("\n[Small Square Buttons]")
    path = os.path.join(ARTS_ROOT, "buttons", "square", "Small Square Buttons.png")
    img = Image.open(path).convert("RGBA")
    cols, rows = find_cells(img)
    print(f"  Grid: {len(cols)} cols x {len(rows)} rows")

    names = ["SL_UI_SmBtn_Lightest", "SL_UI_SmBtn_Light",
             "SL_UI_SmBtn_Medium", "SL_UI_SmBtn_Dark"]
    for ri, (ry0, ry1) in enumerate(rows):
        for ci, (cx0, cx1) in enumerate(cols):
            cell = img.crop((cx0, ry0, cx1, ry1))
            idx = ri * len(cols) + ci
            if idx < len(names):
                save(cell, names[idx])


def extract_coins():
    """Extract coin sprites from coins.png."""
    print("\n[Coins]")
    path = os.path.join(ARTS_ROOT, "Icons", "special icons", "coins.png")
    img = Image.open(path).convert("RGBA")
    cols, rows = find_cells(img)
    print(f"  Grid: {len(cols)} cols x {len(rows)} rows")
    print(f"  Cols: {cols}, Rows: {rows}")

    if len(rows) >= 2 and len(cols) >= 2:
        cx0, cx1 = cols[1]
        ry0, ry1 = rows[1]
        cell = img.crop((cx0, ry0, cx1, ry1))
        save(cell, "SL_UI_Coin")
    if len(rows) >= 1 and len(cols) >= 1:
        cx0, cx1 = cols[0]
        ry0, ry1 = rows[0]
        cell = img.crop((cx0, ry0, cx1, ry1))
        save(cell, "SL_UI_Coin_Small")


def extract_round_buttons():
    """Extract round buttons - one per color row."""
    print("\n[Round Buttons]")
    path = os.path.join(ARTS_ROOT, "buttons", "round", "medium colored round buttons.png")
    img = Image.open(path).convert("RGBA")
    cols, rows = find_cells(img)
    print(f"  Grid: {len(cols)} cols x {len(rows)} rows")

    color_names = [
        "Beige", "Tan", "Brown",
        "Purple", "Lavender", "Blue",
        "Teal", "Mint", "Green",
        "Lime", "Yellow", "Pink", "Rose"
    ]
    for ri, (ry0, ry1) in enumerate(rows):
        if ri >= len(color_names):
            break
        for ci, (cx0, cx1) in enumerate(cols):
            cell = img.crop((cx0, ry0, cx1, ry1))
            suffix = ["", "_Mid", "_Dark"][ci] if ci < 3 else f"_{ci}"
            save(cell, f"SL_UI_Round_{color_names[ri]}{suffix}")


def copy_dialog_box():
    """Copy dialog box panel for 9-slice use."""
    print("\n[Dialog Box Panel]")
    src = os.path.join(ARTS_ROOT, "Dialouge UI", "dialog box.png")
    img = Image.open(src).convert("RGBA")
    save(img, "SL_UI_Panel")


def copy_close_buttons():
    """Copy X close button sprites."""
    print("\n[Close Buttons]")
    xs_dir = os.path.join(ARTS_ROOT, "Other UI sprites", "Xs and check marks", "1s")
    for name, out_name in [
        ("X.png", "SL_UI_Close"),
        ("X pressed.png", "SL_UI_Close_Pressed"),
        ("darker X.png", "SL_UI_Close_Dark"),
        ("darker X prssed.png", "SL_UI_Close_Dark_Pressed"),
    ]:
        path = os.path.join(xs_dir, name)
        if os.path.exists(path):
            img = Image.open(path).convert("RGBA")
            save(img, out_name)


if __name__ == "__main__":
    print(f"Output: {OUT_DIR}")

    copy_dialog_box()
    extract_square_buttons()
    extract_small_square_buttons()
    extract_coins()
    extract_round_buttons()
    copy_close_buttons()

    print(f"\nDone! Total files: {len([f for f in os.listdir(OUT_DIR) if f.endswith('.png')])}")
