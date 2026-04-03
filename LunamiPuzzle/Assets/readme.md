# Lunamify 框架说明

本文档基于当前项目代码整理，目标是说明 `Core`、`GamePlay` 这套 Lunamify 框架在本项目中的功能实现、代码架构，以及业务层如何调用这套框架。

## 1. 项目分层

当前 `Assets/Scripts` 可以按职责分成三层：

- `Core`：底层通用能力，包含单例、事件、输入、UI、资源加载、场景切换、存档、对象池、日志、编辑器工具。
- `GamePlay`：玩法骨架层，封装背包、交互、小游戏、本地化、对象状态管理、统一存档数据结构。
- `Repo`：项目实现层，放具体事件枚举、UI 面板、可交互物体、具体物品、CSV 生成结果等。

可以把它理解成：

- `Core` 负责“怎么做”。
- `GamePlay` 负责“游戏机制长什么样”。
- `Repo` 负责“这个项目里具体做什么”。

## 2. 启动链路

当前主入口场景是 [`Main.unity`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scenes/Main.unity)，Build Settings 中启用了：

启动时的关键流程：

1. `Main` 场景中的各类管理器通过 [`SingletonMono<T>`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/SingletonMono.cs) 初始化，并通过 `DontDestroyOnLoad` 常驻。
2. [`GameManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/GameManager.cs) 在 `OnAwake()` 中初始化 `Csv`，把生成的表数据载入静态 Store。
3. 各个需要存档的系统在 `Start()` 中通过 [`ISaveable.Register()`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/SaveLoad/ISaveable.cs) 注册到 [`SaveLoadManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/SaveLoad/SaveLoadManager.cs)。
4. `GameManager.Start()` 打开菜单面板 `MenuPanel`。
5. 玩家点击“新游戏”或“继续游戏”后，分别走事件驱动的新开局流程，或走存档反序列化流程。

## 3. Core 层功能

### 3.1 单例与常驻对象

- [`SingletonMono<T>`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/SingletonMono.cs) 用于场景中的 MonoBehaviour 单例。
- [`Singleton<T>`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Singleton.cs) 用于纯 C# 类单例。

当前常驻管理器基本都继承 `SingletonMono<T>`，例如：

- `GameManager`
- `SaveLoadManager`
- `TransitionManager`
- `ItemManager`
- `ObjectManager`
- `MiniGameController`
- `CanvasControl`
- `InputModule`
- `CameraControl`
- `SmartCursor`

### 3.2 事件系统

事件系统由 [`EventModule`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Event/EventModule.cs) 和 [`EventSender<Tkey, TValue>`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Event/EventSender.cs) 组成。

特点：

- 使用 `Enum` 作为事件键。
- 参数统一走 `object`。
- 管理器和玩法模块之间通过监听/派发解耦。

当前项目定义的事件在 [`EventName.cs`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/Event/EventName.cs)，例如：

- `EvtStartGameEvent`：新游戏开始。
- `EvtBeforeUnloadScene` / `EvtAfterLoadScene`：场景切换前后。
- `EvtUpdateItem` / `EvtItemUse`：物品拾取和消耗。
- `EvtPassGameEvent` / `EvtFinishMiniGame`：小游戏完成。
- `EvtRefreshBag`：背包 UI 刷新。

这套方式是 Lunamify 的核心调用模式：模块不直接强耦合调用，而是尽量通过事件广播状态变化。

### 3.3 UI 框架

UI 系统核心是：

- [`UIBase`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/UI/UIBase.cs)：所有面板基类。
- [`UIModule`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/UI/UIModule.cs)：UI 栈、面板缓存、实例化与销毁管理。
- [`CanvasControl`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/UI/CanvasControl.cs)：提供 UI 挂载父节点。
- [`PanelPath`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/UI/PanelPath.cs)：面板名到 `Resources` 路径的映射。

调用方式：

1. 面板脚本继承 `UIBase`，实现 `panelName`。
2. 在 `PanelPath.path` 中注册预制体路径。
3. 通过 `UIModule.Instance.OpenPanel<T>()` 或 `PopPanel<T>()` 打开面板。
4. 面板生命周期重写 `DoStart / DoEnable / DoDisable / DoDestroy`。

项目里的例子：

- [`MenuPanel`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/UI/Panels/MenuPanel.cs)
- [`BagPanel`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/UI/Panels/Bag/BagPanel.cs)

### 3.4 输入系统

输入系统由：

- [`InputReader`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Input/InputReader.cs)：`ScriptableObject` 形式的输入桥接器。
- [`InputModule`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Input/InputModule.cs)：对外暴露输入事件订阅接口。

调用方式：

- 在场景中放置 `InputModule`，挂入 `InputReader.asset`。
- 业务层通过 `AddInteractEvent`、`AddLeftEvent` 等方法订阅。
- 鼠标位置通过 `InputModule.Instance.MousePosition` 获取。

### 3.5 场景切换

场景切换由 [`TransitionManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Transition/TransitionManager.cs) 负责。

它做了几件事：

- 控制淡入淡出。
- 卸载旧场景，Additive 加载新场景。
- 在切换前派发 `EvtBeforeUnloadScene`。
- 在切换后派发 `EvtAfterLoadScene`。
- 通过存档记住当前活动场景。

这让对象状态恢复、背包刷新、小游戏状态回填都能挂在统一的场景生命周期上。

### 3.6 存档系统

存档系统核心是：

- [`ISaveable`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/SaveLoad/ISaveable.cs)
- [`SaveLoadManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/SaveLoad/SaveLoadManager.cs)
- [`GamePlay/SaveData`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/SaveData/SaveData.cs) 及其各个 `partial class`

实现方式：

- 每个系统实现 `GenerateSaveData()` 和 `ReadGameData()`。
- 在 `Start()` 中调用 `Register()`。
- 存档时，`SaveLoadManager` 以“类名 -> SaveData”的字典写入 `data.sav`。
- 读档时，再按注册顺序把对应 `SaveData` 回填给各系统。

当前接入的系统包括：

- `GameManager`
- `ItemManager`
- `ObjectManager`
- `MiniGameController`
- `TransitionManager`

### 3.7 CSV 数据驱动

CSV 数据驱动由三部分组成：

- [`CsvGenerator`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Editors/Csv/CsvGenerator.cs)：Editor 下读取 `./csv` 并生成代码。
- [`CsvLoader`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Csv/CsvLoader.cs)：反射装配成强类型字典。
- `Repo/#Generated/CSV`：生成后的表类、Store、Name 常量和 `Csv.cs` 入口。

运行时由 `GameManager` 初始化 `Csv`，之后业务代码直接访问：

- `Csv.ItemCfgStore`
- `Csv.InteractionCfgStore`
- `Csv.LocalizationGameplayCfgStore`
- `Csv.LocalizationUICfgStore`

### 3.8 其他基础能力

- [`Pool<T>`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Pools.cs)：泛型对象池，当前用于背包格子复用。
- [`ResourceManager<T>`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/ResourceManager.cs)：统一 `Resources.Load` 封装。
- [`SmartCursor`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Cursor/SmartCursor.cs)：根据 UI 状态和场景射线切换鼠标形态。
- [`GameLogger`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Logger/GameLogger.cs)：日志打印和可选上传。
- [`SceneNameAttribute`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Core/Editors/CustomAttributes/SceneNameAttribute.cs)：让 Inspector 中字符串字段以下拉形式选择 Build Settings 中的场景名。

## 4. GamePlay 层功能

### 4.1 GameManager

[`GameManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/GameManager.cs) 是玩法总控，主要职责：

- 初始化 CSV。
- 记录当前周目 `gameWeek`。
- 打开初始菜单 UI。
- 处理新游戏事件。
- 在场景加载完成后通知小游戏系统按周目刷新状态。
- 保存语言和当前周目。

### 4.2 背包与物品系统

主要脚本：

- [`ItemManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/Bag/ItemManager.cs)
- [`Item`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/Bag/Logic/Item.cs)
- [`ItemDetail`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/Bag/Data/ItemData.cs)
- [`BagPanel`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/UI/Panels/Bag/BagPanel.cs)
- [`BagItem`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/UI/Panels/Bag/BagItem.cs)

实现要点：

- 场景中的 `Item` 被点击后，隐藏自身、加入背包、发送 `EvtUpdateItem`。
- 背包 UI 监听 `EvtRefreshBag`，按当前背包状态刷新格子。
- 玩家可在 UI 中选择“手持物品”，并把它用于场景交互或物品合成。
- 物品配置由 `ItemCfg` 驱动，包含图标、可堆叠、目标物、结果物等。

### 4.3 场景交互系统

核心脚本：

- [`Interaction`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/Interaction/Interaction.cs)
- [`MailBox`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/Repo/Interacts/MailBox.cs)

机制：

- `Interaction` 代表“不能被捡进背包、但可被某个物品触发”的场景对象。
- 点击时会检查当前手持物品是否匹配 `InteractionCfg.target`。
- 匹配后执行 `OnItemClick()`，并派发 `EvtItemUse` 消耗物品。
- 具体效果由业务层子类实现，例如 `MailBox` 在交互成功后给玩家奖励物品。

### 4.4 小游戏系统

核心脚本：

- [`MiniGame`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/MiniGame/MiniGame.cs)
- [`MiniGameController`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/MiniGame/MiniGameController.cs)

机制：

- `MiniGame` 是场景中的小游戏节点基类。
- `MiniGameController` 负责记录每个小游戏场景是否通关。
- 场景切换完成后，控制器会扫描当前场景中的 `MiniGame`，根据 `gameWeek` 和通关状态回填表现。
- 若小游戏已通关，可自动禁用碰撞、执行 `finishGame` 回调并隐藏对象。

### 4.5 对象状态管理

[`ObjectManager`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/ObjectManager/ObjectManager.cs) 负责维护两类场景状态：

- 场景中 `Item` 是否已被拾取。
- 场景中 `Interaction` 是否已完成。

它借助：

- `EvtBeforeUnloadScene`：离场前扫描场景并写入状态字典。
- `EvtAfterLoadScene`：进场后按状态字典恢复对象启用状态和交互完成状态。

这部分是本项目跨场景持续性的关键。

### 4.6 本地化

本地化核心是：

- [`Localization`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/Localization/Localization.cs)
- [`LangTmp`](/home/russell/Git-projects/puzzle/LunamiPuzzle/Assets/Scripts/GamePlay/Localization/LangTmp.cs)

实现方式：

- `Localization` 维护当前语言枚举和刷新事件。
- `LangTmp` 绑定在 `TextMeshProUGUI` 上，通过 `key` 从 `LocalizationUICfg` 读取文案。
- 切换语言时统一触发 `LanguageUpdate` 刷新 UI 文本。

## 5. Repo 层的职责

`Repo` 不是框架底座，而是“项目落地层”。它的职责是把框架提供的扩展点真正填上内容。

当前已经落地的内容包括：

- `Repo/Event`：项目事件枚举。
- `Repo/UI`：实际面板、路径映射、背包 UI。
- `Repo/Interacts`：具体交互对象，如 `MailBox`。
- `Repo/Items`：具体物品脚本，如 `Item_Key`。
- `Repo/#Generated/CSV`：由表驱动生成的强类型代码。

这意味着 Lunamify 的推荐使用方式是：通用能力留在 `Core/GamePlay`，具体项目内容尽量放到 `Repo`。

## 6. 框架调用方式

### 6.1 新增一个 UI 面板

1. 新建面板预制体到 `Assets/Resources/...`。
2. 新建脚本继承 `UIBase`。
3. 在 `PanelPath` 中注册 `PanelName -> Resources 路径`。
4. 通过 `UIModule.Instance.OpenPanel<YourPanel>(PanelName.YourPanel)` 打开。

### 6.2 新增一个可拾取物品

1. 新增 `csv` 中的物品配置，重新执行 `Csv/Generate Csv`。
2. 场景对象挂 `Item` 或其子类。
3. 在 Inspector 中通过 `ItemEditor` 选择配置 ID。
4. 需要特殊逻辑时继承 `Item`，重写 `DoCompositeItem()` 或 `OnInteractClick()`。

### 6.3 新增一个可交互对象

1. 在 `InteractionCfg` 中新增配置并重新生成 CSV。
2. 场景对象挂 `Interaction` 或其子类。
3. 用 `InteractionEditor` 选择交互 ID。
4. 需要自定义效果时重写 `OnItemClick()`。

### 6.4 新增一个可存档系统

1. 让脚本实现 `ISaveable`。
2. 定义对应的 `SaveData` 字段，通常放在 `GamePlay/SaveData` 的 `partial class SaveData` 中。
3. 实现 `GenerateSaveData()` 与 `ReadGameData()`。
4. 在 `Start()` 中调用 `Register()`。

### 6.5 新增一个小游戏

1. 新建脚本继承 `MiniGame`。
2. 实现 `OnInitMiniGame()`、`OnChooseGameData()`、`OnCheckGameStateEvent()` 等虚方法。
3. 配置 `gameScene` 和 `finishGame`。
4. 通过事件 `EvtPassGameEvent` / `EvtFinishMiniGame` 接入项目流程。

### 6.6 新增一类配置表

1. 在项目根目录 `./csv` 新增 `.csv`。
2. 使用 `字段名&类型` 作为表头格式，`id` 作为主键首列。
3. 执行菜单 `Csv/Generate Csv`。
4. 运行时通过 `Csv.xxxStore` 读取。

## 7. 这套框架的核心设计思路

Lunamify 在当前项目中的设计重点不是复杂的抽象层，而是“用少量统一规范把玩法模块拼起来”：

- 单例常驻管理器承担全局系统职责。
- 事件总线负责模块解耦。
- `UIBase + UIModule` 统一 UI 生命周期。
- `ISaveable + SaveLoadManager` 统一存档接入。
- `Csv + Editor` 让玩法数据配置化。
- `GamePlay` 提供可继承的玩法骨架。
- `Repo` 专注项目内容实现。

如果后续要把 Lunamify 真正抽成独立框架，当前代码最适合继续沉淀到框架层的目录是：

- `Core`
- `GamePlay`
- 与之配套的 Editor 工具、CSV 工具、输入配置和通用资源配置

而 `Repo` 更适合作为具体游戏项目模板或示例工程保留。
