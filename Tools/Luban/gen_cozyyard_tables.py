"""
生成 CozyYard 退休小院 Luban 配置表 Excel 文件
运行: python gen_cozyyard_tables.py
"""
import os
from openpyxl import Workbook
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side

DATAS_DIR = os.path.join(os.path.dirname(__file__), "DataTables", "Datas", "CozyYard")

HEADER_FONT = Font(bold=True, size=11)
META_FILL = PatternFill(start_color="D9E1F2", end_color="D9E1F2", fill_type="solid")
COMMENT_FILL = PatternFill(start_color="E2EFDA", end_color="E2EFDA", fill_type="solid")
THIN_BORDER = Border(
    left=Side(style="thin"),
    right=Side(style="thin"),
    top=Side(style="thin"),
    bottom=Side(style="thin"),
)


def style_meta_rows(ws, num_cols, num_meta_rows=3):
    for row_idx in range(1, num_meta_rows + 1):
        fill = META_FILL if row_idx <= 2 else COMMENT_FILL
        for col_idx in range(1, num_cols + 1):
            cell = ws.cell(row=row_idx, column=col_idx)
            cell.font = HEADER_FONT
            cell.fill = fill
            cell.border = THIN_BORDER
            cell.alignment = Alignment(horizontal="center")


def auto_width(ws):
    for col in ws.columns:
        max_len = 0
        col_letter = col[0].column_letter
        for cell in col:
            val = str(cell.value) if cell.value is not None else ""
            max_len = max(max_len, len(val.encode("utf-8")))
        ws.column_dimensions[col_letter].width = min(max_len + 4, 30)


def write_sheet(ws, headers, comments, rows):
    ws.append(["##var"] + headers[1:])
    ws.append(["##"] + comments[1:])
    for row in rows:
        ws.append(row)
    style_meta_rows(ws, len(headers), num_meta_rows=2)
    auto_width(ws)


def create_tables_xlsx():
    """__tables__.xlsx - Luban 表定义"""
    wb = Workbook()
    ws = wb.active
    ws.title = "tables"

    cols = ["##var", "full_name", "value_type", "read_schema_from_file", "input", "mode", "index", "group", "comment", "output", "tags"]
    ws.append(cols)

    tables = [
        ["", "TbItem",       "Item",       "false", "item.xlsx",       "map", "id", "", "物品总表",     "", ""],
        ["", "TbCrop",       "Crop",       "false", "crop.xlsx",       "map", "id", "", "作物配置表",   "", ""],
        ["", "TbTree",       "Tree",       "false", "tree.xlsx",       "map", "id", "", "树木配置表",   "", ""],
        ["", "TbAnimal",     "Animal",     "false", "animal.xlsx",     "map", "id", "", "动物配置表",   "", ""],
        ["", "TbBuilding",   "Building",   "false", "building.xlsx",   "map", "id", "", "建筑配置表",   "", ""],
        ["", "TbRecipe",     "Recipe",     "false", "recipe.xlsx",     "map", "id", "", "制作配方表",   "", ""],
        ["", "TbVisitor",    "Visitor",    "false", "visitor.xlsx",    "map", "id", "", "来客配置表",   "", ""],
        ["", "TbOrder",      "Order",      "false", "order.xlsx",      "map", "id", "", "订单模板表",   "", ""],
        ["", "TbMilestone",  "Milestone",  "false", "milestone.xlsx",  "map", "id", "", "里程碑表",     "", ""],
        ["", "TbSeason",     "Season",     "false", "season.xlsx",     "map", "id", "", "季节表",       "", ""],
        ["", "TbTime",       "TimeCfg",    "false", "time.xlsx",       "map", "id", "", "时间配置表",   "", ""],
        ["", "TbExpansion",  "Expansion",  "false", "expansion.xlsx",  "map", "id", "", "扩建区域表",   "", ""],
        ["", "TbObstacle",   "Obstacle",   "false", "obstacle.xlsx",   "map", "id", "", "障碍物表",     "", ""],
        ["", "TbShop",       "ShopItem",   "false", "shop.xlsx",       "map", "id", "", "商店商品表",   "", ""],
        ["", "TbUIWindow",   "UIWindow",   "false", "uiwindow.xlsx",   "map", "id", "", "UI窗口表",     "", ""],
        ["", "TbLanguage",   "Language",   "false", "language.xlsx",   "map", "key", "", "多语言表",    "", ""],
    ]

    for t in tables:
        ws.append(t)

    for col_idx in range(1, len(cols) + 1):
        cell = ws.cell(row=1, column=col_idx)
        cell.font = HEADER_FONT
        cell.fill = META_FILL
        cell.border = THIN_BORDER
    auto_width(ws)

    path = os.path.join(DATAS_DIR, "__tables__.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_obstacle_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "obstacle"

    headers  = ["##var", "id", "name",   "clearTime", "dropItemId", "dropQuantity"]
    comments = ["##",    "ID", "名称",    "清除耗时(分钟)", "掉落物品ID", "掉落数量"]

    rows = [
        ["", 1, "杂草",  15, 1001, 2],
        ["", 2, "石头",  30, 1002, 3],
        ["", 3, "树桩",  60, 1003, 5],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "obstacle.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_item_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "item"

    headers  = ["##var", "id",   "name",    "type",  "stackLimit", "desc"]
    comments = ["##",    "ID",   "名称",     "类型",   "堆叠上限",    "描述"]

    rows = [
        ["", 1001, "杂草纤维", "Material", 99, "清除杂草获得"],
        ["", 1002, "石头",     "Material", 99, "清除石块获得"],
        ["", 1003, "木材",     "Material", 99, "清除树桩获得"],
        ["", 2001, "白菜种子", "Seed", 50, "种植白菜"],
        ["", 2002, "萝卜种子", "Seed", 50, "种植萝卜"],
        ["", 2003, "糯米种子", "Seed", 50, "种植糯米"],
        ["", 2004, "菊花种子", "Seed", 50, "种植菊花"],
        ["", 2005, "辣椒种子", "Seed", 50, "种植辣椒"],
        ["", 3001, "白菜",   "Product", 50, "新鲜白菜"],
        ["", 3002, "萝卜",   "Product", 50, "新鲜萝卜"],
        ["", 3003, "糯米",   "Product", 50, "饱满的糯米"],
        ["", 3004, "菊花",   "Product", 50, "新鲜菊花"],
        ["", 3005, "辣椒",   "Product", 50, "新鲜辣椒"],
        ["", 3101, "鸡蛋",   "Product", 50, "新鲜鸡蛋"],
        # Tree produce
        ["", 3006, "桂花",   "Product", 50, "秋天采集"],
        ["", 3007, "柿子",   "Product", 50, "秋天采集"],
        # Intermediate materials
        ["", 4001, "桂花干", "Material", 50, "晾晒桂花制得"],
        ["", 4002, "糯米粉", "Material", 50, "石磨糯米制得"],
        ["", 4003, "萝卜干", "Material", 50, "晾晒萝卜制得"],
        ["", 4004, "菊花干", "Material", 50, "晾晒菊花制得"],
        # Final products
        ["", 5001, "桂花糕", "Product", 20, "香甜的桂花糕"],
        ["", 5002, "辣炒蛋", "Product", 20, "简单美味"],
        ["", 5003, "清炒白菜", "Product", 20, "清淡爽口"],
        ["", 5004, "菊花茶", "Product", 20, "清香提神"],
        ["", 5005, "柿饼",   "Product", 20, "甜糯柿饼"],
        # Junk
        ["", 9001, "黑暗料理", "Material", 10, "实验失败的产物"],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "item.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_crop_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "crop"

    headers  = ["##var", "id", "name", "season", "growthDays", "harvestWindow", "seedItemId", "produceItemId", "produceQuantity"]
    comments = ["##",    "ID", "名称",  "适宜季节(0春1夏2秋3冬)", "生长天数", "收获窗口(天)", "种子物品ID", "产出物品ID", "产出数量"]

    rows = [
        ["", 1, "白菜", 2, 3, 4, 2001, 3001, 2],
        ["", 2, "萝卜", 2, 5, 4, 2002, 3002, 2],
        ["", 3, "糯米", 2, 7, 3, 2003, 3003, 3],
        ["", 4, "菊花", 2, 5, 5, 2004, 3004, 2],
        ["", 5, "辣椒", 2, 5, 4, 2005, 3005, 3],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "crop.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_uiwindow_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "uiwindow"

    headers  = ["##var", "id", "desc", "windowName", "isNeedBlackMask", "isClickBlankQuit", "enterAnimType", "exitAnimType", "isIgnoreSafeArea", "uiLayer"]
    comments = ["##",    "ID", "描述",  "窗口名称",    "需要黑色遮罩",     "点击空白关闭",       "进入动画",       "退出动画",       "忽略安全区域",      "UI层级"]

    rows = [
        ["", 1001, "游戏HUD",     "GameHUD",           False, False, 0, 0, True, 1],
        ["", 1002, "背包",         "InventoryWindow",   True,  True,  1, 1, False, 2],
        ["", 1003, "建造面板",     "BuildWindow",       True,  True,  1, 1, False, 2],
        ["", 1004, "制作界面",     "CraftWindow",       True,  True,  1, 1, False, 2],
        ["", 1005, "来客对话",     "VisitorWindow",     True,  True,  1, 1, False, 2],
        ["", 1006, "里程碑",       "MilestoneWindow",   True,  True,  1, 1, False, 2],
        ["", 1007, "配方本",       "RecipeBookWindow",  True,  True,  1, 1, False, 2],
        ["", 1008, "问妈",         "PhoneWindow",       True,  True,  1, 1, False, 2],
        ["", 1009, "货郎商店",     "ShopWindow",        True,  True,  1, 1, False, 2],
        ["", 1010, "设置",         "SettingsWindow",    True,  True,  1, 1, False, 2],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "uiwindow.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_time_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "time"

    headers  = ["##var", "id", "phaseName", "startMinute", "lightIntensity", "lightColor"]
    comments = ["##",    "ID", "时段名称",    "开始分钟",     "光照强度(0-1)",    "光照颜色(hex)"]

    rows = [
        ["", 1, "Dawn",      360,  0.4, "FFD4A0"],
        ["", 2, "Morning",   480,  0.8, "FFFFFF"],
        ["", 3, "Noon",      720,  1.0, "FFFFFF"],
        ["", 4, "Afternoon", 840,  0.9, "FFF8E0"],
        ["", 5, "Evening",   1080, 0.5, "FF9040"],
        ["", 6, "Night",     1260, 0.2, "4060A0"],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "time.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_season_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "season"

    headers  = ["##var", "id", "name",   "days", "tempModifier"]
    comments = ["##",    "ID", "季节名称", "天数",  "温度修正"]

    rows = [
        ["", 0, "春", 15, 1.0],
        ["", 1, "夏", 15, 1.2],
        ["", 2, "秋", 15, 1.0],
        ["", 3, "冬", 10, 0.5],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "season.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_animal_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "animal"

    headers  = ["##var", "id", "name", "type", "produceItemId", "produceCycleDays", "requiredBuildingId", "feedItemId", "feedQuantity"]
    comments = ["##",    "ID", "名称",  "类型(Poultry/Aquatic/Pet)", "产出物品ID", "产出周期(天)", "需要设施ID", "饲料物品ID", "每次喂食量"]

    rows = [
        ["", 1, "鸡", "Poultry", 3101, 2, 40, 1001, 2],
        ["", 2, "猫", "Pet",     0,    0, 0,  0,    0],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "animal.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_building_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "building"

    headers  = ["##var", "id", "name", "category", "sizeX", "sizeY", "materials", "materialQtys", "buildTime", "prerequisiteId", "level"]
    comments = ["##",    "ID", "名称",  "类别",      "宽",    "高",    "材料ID列表", "材料数量列表",   "建造时间(分钟)", "前置建筑ID", "等级"]

    rows = [
        ["", 1,  "茅草屋",   "House",      2, 2, "1003",      "20",     120, 0,  1],
        ["", 2,  "土砖房",   "House",      3, 3, "1003,1002", "30,20",  180, 1,  2],
        ["", 10, "野外篝火", "Production", 1, 1, "1003,1002", "5,3",    30,  0,  1],
        ["", 11, "土灶",     "Production", 1, 1, "1002,1003", "10,8",   60,  1,  2],
        ["", 20, "简易竹架", "Production", 1, 1, "1003",      "8",      30,  0,  1],
        ["", 30, "石磨",     "Production", 1, 1, "1002",      "15",     60,  0,  1],
        ["", 40, "露天围栏", "Livestock",  2, 2, "1003",      "12",     45,  0,  1],
        ["", 50, "围栏",     "Decoration", 1, 1, "1003",      "3",      10,  0,  1],
        ["", 60, "饲料槽",   "Functional", 1, 1, "1003,1002", "5,3",    20,  0,  1],
        ["", 70, "仓库",     "Functional", 2, 2, "1003,1002", "15,10",  90,  0,  1],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "building.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_recipe_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "recipe"

    headers  = ["##var", "id", "name", "requiredBuildingId", "inputItemIds", "inputQuantities", "outputItemId", "outputQuantity", "craftMinutes"]
    comments = ["##",    "ID", "名称",  "需要设施ID",          "输入物品ID列表", "输入数量列表",     "输出物品ID",    "输出数量",       "制作时间(分钟)"]

    rows = [
        ["", 1,  "桂花干",     20, "3006",      "3",   4001, 2, 120],
        ["", 2,  "糯米粉",     30, "3003",      "2",   4002, 2, 60],
        ["", 3,  "桂花糕",     10, "4001,4002", "2,2", 5001, 1, 90],
        ["", 4,  "辣炒蛋",     10, "3101,3005", "1,1", 5002, 1, 30],
        ["", 5,  "清炒白菜",   10, "3001",      "2",   5003, 1, 20],
        ["", 6,  "萝卜干",     20, "3002",      "2",   4003, 2, 120],
        ["", 7,  "菊花干",     20, "3004",      "3",   4004, 2, 120],
        ["", 8,  "菊花茶",     10, "4004",      "2",   5004, 1, 30],
        ["", 9,  "柿饼",       20, "3007",      "3",   5005, 2, 180],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "recipe.xlsx")
    wb.save(path)
    print(f"  -> {path}")


def create_language_xlsx():
    wb = Workbook()
    ws = wb.active
    ws.title = "language"

    headers  = ["##var", "key", "cn"]
    comments = ["##",    "键名", "中文"]

    rows = [
        ["", "season_spring", "春"],
        ["", "season_summer", "夏"],
        ["", "season_autumn", "秋"],
        ["", "season_winter", "冬"],
        ["", "phase_dawn",    "清晨"],
        ["", "phase_morning", "上午"],
        ["", "phase_noon",    "正午"],
        ["", "phase_afternoon", "下午"],
        ["", "phase_evening", "傍晚"],
        ["", "phase_night",   "夜晚"],
    ]

    write_sheet(ws, headers, comments, rows)
    path = os.path.join(DATAS_DIR, "language.xlsx")
    wb.save(path)
    print(f"  -> {path}")


if __name__ == "__main__":
    os.makedirs(DATAS_DIR, exist_ok=True)
    print("Generating CozyYard Luban Excel files...")
    create_tables_xlsx()
    create_obstacle_xlsx()
    create_item_xlsx()
    create_crop_xlsx()
    create_animal_xlsx()
    create_building_xlsx()
    create_recipe_xlsx()
    create_uiwindow_xlsx()
    create_language_xlsx()
    create_time_xlsx()
    create_season_xlsx()
    print("Done!")
