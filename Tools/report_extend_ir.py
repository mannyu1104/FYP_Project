import html
import os
import re
import shutil
import zipfile
from pathlib import Path


INPUT = Path(r"C:\Users\funch\Desktop\FunChengJun_TP071586_FYPDocument.docx")
OUTPUT = Path(r"E:\APU\Year 3\FYP\Project\FYP_Project\output\report\FunChengJun_TP071586_FinalReport_Draft.docx")


CONTENT = [
    ("h1", "CHAPTER 4: SYSTEM DESIGN AND IMPLEMENTATION"),
    ("h2", "4.1 Introduction"),
    ("p", "This chapter describes the design and implementation of the investigation game prototype developed for this Final Year Project. The prototype was created to apply the findings from the literature review and questionnaire analysis into a playable Unity-based experience. The implementation focuses on supporting object discovery and recognition through spatial level design, strategic clue placement, environmental context, and controlled interaction feedback."),
    ("p", "The purpose of the prototype is not to produce a full commercial investigation game, but to demonstrate how level layout, clue placement, interactable objects, and narrative interaction can guide players without relying entirely on direct UI-based guidance. The prototype therefore acts as a practical response to the research aim and user requirements identified in Chapter 3."),
    ("h2", "4.2 Overall Game Concept"),
    ("p", "The game prototype is designed as a narrative-driven investigation experience. The player explores several connected locations, interacts with objects and NPCs, reads contextual information, and uses an in-game computer interface to support investigation activities. The core experience encourages the player to observe the environment carefully and identify meaningful objects based on their placement, visual appearance, and relationship to the story."),
    ("p", "The prototype uses a panel-based scene structure in Unity. Instead of loading many separate Unity scenes, the main gameplay is organised under a single main scene, with different locations represented as panels. This approach allows the project to switch between areas quickly while keeping shared systems such as dialogue, settings, audio, save/load, map navigation, and localisation active in one controlled environment."),
    ("h2", "4.3 Implemented Game Areas"),
    ("p", "The main playable environment includes the home area, orphanage area, orphanage entrance, staff room, and additional map destinations prepared for the investigation flow. Each area is represented using a separate panel containing background artwork, interactable objects, navigation buttons, and relevant NPCs. The home area introduces the player to the investigation flow and contains the computer interaction. The orphanage-related panels support progression through the case by requiring the player to move from the orphanage to the entrance and then to the staff room."),
    ("p", "The level flow was designed to control the order in which information is encountered. For example, the player does not immediately access every location at once. Instead, movement is structured through navigation buttons and map panels so that the investigation can develop gradually. This supports the research requirement for meaningful object placement and environmental progression."),
    ("h2", "4.4 Object Interaction System"),
    ("p", "Interactable objects are used as the main method for discovery and recognition. Objects placed within the environment can respond to player interaction by showing dialogue text, object descriptions, or inspection images. This allows the player to examine clues and understand their relevance through interaction rather than through constant explicit instruction."),
    ("p", "The cursor interaction system provides visual feedback when the player hovers over interactive elements. This improves usability while still allowing the environment itself to carry the main guidance role. Different interaction targets can use different cursor presets, making it easier for the player to recognise whether an object can be viewed, talked to, or used for another action."),
    ("h2", "4.5 Dialogue System"),
    ("p", "The dialogue system supports conversations with NPCs and descriptive text for objects. It includes functions such as dialogue progression, history viewing, automatic playback, skip functionality, and continued NPC conversation progress. These features allow narrative information to be delivered in a structured way while maintaining player control over reading speed."),
    ("p", "The dialogue interface was improved during implementation by adding an inline continuation indicator. When a sentence has finished displaying and more dialogue remains, a small downward triangle appears at the end of the current sentence to show that the player can continue. This indicator does not appear on the final sentence, reducing confusion and improving readability."),
    ("h2", "4.6 Computer Interface System"),
    ("p", "The prototype includes an in-game computer interface that simulates opening a separate desktop environment. When the player interacts with the computer in the main game, the main gameplay canvas is hidden and the computer canvas is shown. This creates the impression that the player has entered the computer screen while still remaining inside the same Unity scene."),
    ("p", "The computer interface contains desktop icons, browser-style pages, and a tutorial application. A return button is shown only on the computer desktop, allowing the player to return to the main gameplay canvas. When an application or browser page is opened, the desktop return button is hidden so that it does not interfere with the computer interface. Browser pages use their own close controls, separating desktop-level navigation from browser-level navigation."),
    ("h2", "4.7 Map and Location Navigation"),
    ("p", "The map and location navigation system controls movement between the different gameplay panels. The design was adjusted so that the orphanage flow follows a clear order: the player first reaches the orphanage, then moves to the entrance, then enters the staff room. From the staff room, the player returns to the entrance before returning to the orphanage. This prevents the player from skipping important spatial context and supports a more coherent investigation route."),
    ("p", "This navigation structure reflects the research findings that clear layout and controlled progression can improve object discovery. By limiting available movement options based on the current location, the game reduces confusion while still allowing the player to feel that they are exploring connected spaces."),
    ("h2", "4.8 Localisation System"),
    ("p", "A localisation system was implemented to support both Chinese and English text. Dialogue lines, object descriptions, item text, and user interface labels can be displayed according to the selected language. The settings menu includes a language selection control, and the selected language is saved so that the game can remember the player's previous choice."),
    ("p", "Localisation is important for accessibility because players may understand clues and story information more accurately in their preferred language. Since investigation games rely heavily on text interpretation, supporting multiple languages helps reduce misunderstanding and improves the clarity of object and clue information."),
    ("h2", "4.9 Audio and Settings System"),
    ("p", "The prototype includes a basic audio management system that separates master volume, background music volume, and sound effect volume. These values can be controlled from the settings interface. This allows players to adjust the audio experience based on preference and supports a more complete game prototype."),
    ("p", "Button sound effects and dialogue sound effects were also considered as part of the interaction feedback. These small audio responses help make the interface feel more responsive and support player engagement during exploration and dialogue reading."),
    ("h2", "4.10 Save and Load Support"),
    ("p", "The project includes save and load support for selected gameplay data. The main menu was adjusted so that a new game and load game are separated into different buttons. The load game option is only intended to appear when save data exists, preventing the player from selecting a load option before any progress has been saved."),
    ("p", "This system supports usability by making the menu state match the actual availability of saved progress. It also helps the game behave more like a complete prototype rather than a single-session demonstration."),
    ("h2", "4.11 Implementation Summary"),
    ("p", "Overall, the implementation translates the research findings into a functional investigation game prototype. The prototype includes spatial panel navigation, interactable objects, NPC dialogue, object inspection, a computer interface, map progression, localisation, audio settings, and basic save/load support. These systems work together to demonstrate how object discovery and recognition can be supported through design choices rather than relying only on direct visual highlighting."),

    ("h1", "CHAPTER 5: TESTING AND EVALUATION"),
    ("h2", "5.1 Introduction"),
    ("p", "Testing was conducted to ensure that the prototype functions correctly and that the implemented features support the project objectives. Since the project focuses on object discovery and recognition in investigation games, the testing process considered both functional correctness and the relationship between the implemented systems and the user requirements identified in Chapter 3."),
    ("h2", "5.2 Functional Testing"),
    ("p", "Functional testing was carried out throughout development to check whether each major gameplay system behaved as intended. The test cases focused on menu flow, map navigation, dialogue progression, object interaction, computer interface switching, localisation, settings, audio control, and save/load behaviour."),
    ("table", [
        ["Test Area", "Expected Result", "Outcome"],
        ["New Game flow", "Selecting New Game hides the main menu and opens the gameplay panel.", "Passed"],
        ["Computer interaction", "Clicking the computer opens the computer canvas and disables the main gameplay canvas.", "Passed"],
        ["Computer return button", "The return button appears on the desktop and hides when an application is opened.", "Passed"],
        ["Dialogue progression", "NPC and object dialogue can progress line by line without breaking conversation continuity.", "Passed"],
        ["Dialogue history", "Previously displayed dialogue can be viewed through the history panel.", "Passed"],
        ["Map navigation", "The player can move between available location panels according to the intended route.", "Passed"],
        ["Orphanage route", "The player moves from orphanage to entrance, then staff room, then back through the correct route.", "Passed"],
        ["Language switching", "UI, dialogue, and object text change according to the selected language.", "Passed"],
        ["Audio settings", "Master, BGM, and SFX volume sliders control the relevant audio categories.", "Passed"],
        ["Load Game visibility", "Load Game is hidden when no save data exists and appears after saving.", "Passed after adjustment"],
    ]),
    ("h2", "5.3 Requirement-Based Evaluation"),
    ("p", "The prototype was evaluated against the user requirements developed from the survey analysis. This evaluation helped determine whether the final implementation addressed the main needs identified from player responses."),
    ("table", [
        ["User Requirement", "Implementation Response"],
        ["Natural Exploration Support", "The game uses location panels, environmental objects, and map progression to encourage exploration."],
        ["Clear Visual Distinction of Objects", "Important objects are represented as interactable elements with visual placement and cursor feedback."],
        ["Meaningful Object Placement", "Objects and NPCs are placed in relevant locations such as the home area, orphanage entrance, and staff room."],
        ["Balanced Level Design", "Panel-based navigation controls the amount of information shown to the player at one time."],
        ["Controlled Environmental Clutter", "Non-essential elements can be hidden or limited during guided progression, especially in the video demonstration flow."],
        ["Optional or Subtle Guidance System", "Cursor feedback and navigation buttons provide guidance without replacing environmental observation."],
        ["Strong Environmental Storytelling", "Dialogue, object descriptions, and computer information support the story context."],
        ["Moderate Challenge Level", "The prototype balances clue discovery with structured navigation so that players are guided but not fully directed."],
    ]),
    ("h2", "5.4 Evaluation of Project Objectives"),
    ("p", "The first objective was to analyse how players discover important objects during exploration in investigation-based games. This was addressed through the questionnaire findings in Chapter 3 and applied in the prototype through interactable objects and controlled spatial layouts."),
    ("p", "The second objective was to examine how players recognise and differentiate relevant objects from non-relevant ones. The prototype addresses this by using visual contrast, object placement, inspection images, and interaction feedback to help players identify objects that may be important to the investigation."),
    ("p", "The third objective was to evaluate how level design elements influence player exploration and object recognition. The implementation uses panel-based level design, route control, location grouping, and environmental context to demonstrate how spatial structure can guide player behaviour."),
    ("p", "The fourth objective was to assess player preferences and experiences regarding object discovery without heavy reliance on UI guidance. The survey results showed that players prefer a balance between exploration and clarity. The prototype responds to this by combining natural environmental design with limited feedback such as cursor changes, dialogue prompts, and map navigation."),
    ("h2", "5.5 Issues Encountered During Development"),
    ("p", "Several implementation issues were encountered during development. One issue was the management of multiple UI panels within a single Unity scene. Since the project uses panel-based scene switching, inactive panels, map panels, dialogue panels, settings panels, and computer panels needed to be carefully controlled to prevent overlapping interactions."),
    ("p", "Another issue was the integration of localisation across different types of text. Dialogue, object descriptions, item text, and user interface labels required consistent handling so that language changes were reflected across the game. Font compatibility also needed to be considered because some fonts did not display Chinese characters correctly."),
    ("p", "The computer interface also required adjustment because the desktop return button and browser-level close buttons had different purposes. The final implementation separates these responsibilities so that the player can exit the computer from the desktop while browser pages use their own close controls."),
    ("h2", "5.6 Testing Summary"),
    ("p", "The testing process shows that the major systems required for the prototype are functioning and support the intended investigation gameplay. While the prototype is not a full commercial game, it provides a working demonstration of the research concept. The implemented systems support object discovery, object recognition, narrative progression, and player guidance through level design and interaction feedback."),

    ("h1", "CHAPTER 6: CONCLUSION AND FUTURE WORK"),
    ("h2", "6.1 Conclusion"),
    ("p", "This Final Year Project investigated how strategic clue placement and spatial level design can enhance object discovery and recognition in investigation-based games. The research began by identifying issues in existing investigation game design, including over-reliance on UI guidance, weak environmental guidance, and difficulty distinguishing relevant objects from non-relevant objects."),
    ("p", "Through literature review and questionnaire analysis, the study found that players rely on visual appearance, object placement, environmental context, and interaction feedback when identifying important objects. The findings also showed that players prefer a balanced approach: experienced players often enjoy exploration and subtle guidance, while less experienced players benefit from clearer cues and structured progression."),
    ("p", "Based on these findings, a Unity-based investigation game prototype was developed. The prototype includes connected location panels, interactable objects, NPC dialogue, object inspection, map navigation, a computer interface, localisation support, audio settings, and save/load functions. These features demonstrate how a game can guide players through environmental structure and meaningful interaction rather than relying only on direct object highlighting."),
    ("p", "Overall, the project achieved its main aim by translating research findings into a functional prototype. The final implementation shows that strategic placement, clear spatial structure, and contextual feedback can support object discovery and recognition while preserving the player's sense of exploration."),
    ("h2", "6.2 Limitations"),
    ("p", "There are several limitations in this project. First, the prototype was developed within a limited timeframe, so the game content, number of locations, and amount of narrative content are limited. Some visual assets and interaction elements are functional placeholders rather than final production-quality assets."),
    ("p", "Second, the evaluation relies mainly on survey findings and functional testing rather than a large-scale gameplay study. Although the survey provided useful insights into player preferences, further direct observation would be needed to measure how players actually behave while playing the prototype."),
    ("p", "Third, because the project uses a single-scene, panel-based structure, the current implementation is suitable for a prototype but may require refactoring if expanded into a larger game with more levels, cases, and complex state management."),
    ("h2", "6.3 Future Work"),
    ("p", "Future work could include expanding the prototype with more investigation cases, additional environments, more complete artwork, and deeper NPC interactions. More advanced clue logic could also be implemented so that player decisions have stronger consequences on the investigation outcome."),
    ("p", "A future version of the project could also include formal playtesting with observation, task completion timing, and post-play interviews. This would allow the researcher to compare intended player guidance with actual player behaviour and further evaluate the effectiveness of strategic clue placement and spatial level design."),
    ("p", "Finally, the localisation, save/load, audio, and computer interface systems could be polished further to support a more complete game experience. These improvements would help transform the current research prototype into a more refined investigation game."),
]


def escape_text(text):
    return html.escape(text, quote=False)


def p_xml(text, style=None):
    style_xml = f'<w:pPr><w:pStyle w:val="{style}"/></w:pPr>' if style else ""
    return f'<w:p>{style_xml}<w:r><w:t xml:space="preserve">{escape_text(text)}</w:t></w:r></w:p>'


def table_xml(rows):
    col_count = max(len(row) for row in rows)
    grid = "".join('<w:gridCol w:w="3000"/>' for _ in range(col_count))
    row_xml = []
    for row in rows:
        cells = []
        for cell in row:
            cells.append(
                '<w:tc><w:tcPr><w:tcW w:w="3000" w:type="dxa"/></w:tcPr>'
                + p_xml(str(cell))
                + '</w:tc>'
            )
        row_xml.append('<w:tr>' + "".join(cells) + '</w:tr>')
    return '<w:tbl><w:tblPr><w:tblW w:w="9000" w:type="dxa"/><w:tblBorders><w:top w:val="single" w:sz="4" w:space="0" w:color="808080"/><w:left w:val="single" w:sz="4" w:space="0" w:color="808080"/><w:bottom w:val="single" w:sz="4" w:space="0" w:color="808080"/><w:right w:val="single" w:sz="4" w:space="0" w:color="808080"/><w:insideH w:val="single" w:sz="4" w:space="0" w:color="808080"/><w:insideV w:val="single" w:sz="4" w:space="0" w:color="808080"/></w:tblBorders></w:tblPr><w:tblGrid>' + grid + '</w:tblGrid>' + "".join(row_xml) + '</w:tbl>'


def build_insert_xml():
    parts = []
    for kind, value in CONTENT:
        if kind == "h1":
            parts.append(p_xml(value, "Heading1"))
        elif kind == "h2":
            parts.append(p_xml(value, "Heading2"))
        elif kind == "table":
            parts.append(table_xml(value))
        else:
            parts.append(p_xml(value))
    return "".join(parts)


def paragraph_text(paragraph_xml):
    texts = re.findall(r"<w:t[^>]*>(.*?)</w:t>", paragraph_xml)
    return html.unescape("".join(texts)).strip()


def main():
    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(INPUT, OUTPUT)

    with zipfile.ZipFile(OUTPUT, "r") as zin:
        files = {name: zin.read(name) for name in zin.namelist()}

    document_xml = files["word/document.xml"].decode("utf-8", errors="ignore")
    match = re.search(r"(<w:body[^>]*>)([\s\S]*?)(<w:sectPr[\s\S]*?</w:sectPr>\s*</w:body>)", document_xml)
    if not match:
        raise RuntimeError("Could not locate document body.")

    body_start, body_content, body_end = match.groups()
    blocks = re.findall(r"<w:p[\s\S]*?</w:p>|<w:tbl[\s\S]*?</w:tbl>", body_content)

    chapter4_index = None
    references_index = None
    for index, block in enumerate(blocks):
        text = paragraph_text(block)
        if chapter4_index is None and text.upper() == "CHAPTER 4: CONCLUSION":
            chapter4_index = index
        if references_index is None and text == "References":
            references_index = index

    if chapter4_index is None or references_index is None or chapter4_index >= references_index:
        raise RuntimeError("Could not find the old Chapter 4 Conclusion and References boundary.")

    new_blocks = blocks[:chapter4_index] + [build_insert_xml()] + blocks[references_index:]
    new_body_content = "".join(new_blocks)

    new_document_xml = document_xml[:match.start()] + body_start + new_body_content + body_end + document_xml[match.end():]

    # Lightly update front matter from IR wording to final report wording.
    replacements = {
        "INVESTIGATION REPORT": "FINAL YEAR PROJECT REPORT",
        "Overall, this study provides insights into how level design can support more intuitive object discovery and recognition, contributing to the development of more engaging and immersive investigation-based gameplay.": "Overall, this study provides insights into how level design can support more intuitive object discovery and recognition. These findings were applied in the development of a Unity-based investigation game prototype that demonstrates exploration, object interaction, dialogue, map navigation, localisation, audio settings, and computer-based investigation features.",
    }
    for old, new in replacements.items():
        new_document_xml = new_document_xml.replace(escape_text(old), escape_text(new))

    files["word/document.xml"] = new_document_xml.encode("utf-8")

    with zipfile.ZipFile(OUTPUT, "w", zipfile.ZIP_DEFLATED) as zout:
        for name, data in files.items():
            zout.writestr(name, data)

    print(str(OUTPUT))


if __name__ == "__main__":
    main()
