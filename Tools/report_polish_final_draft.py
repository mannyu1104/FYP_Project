import html
import shutil
import zipfile
from pathlib import Path


DOCX = Path(r"E:\APU\Year 3\FYP\Project\FYP_Project\output\report\FunChengJun_TP071586_FinalReport_Draft.docx")


REPLACEMENTS = {
    "This Investigation Report presents a study on how level design can enhance object discovery and recognition in investigation-based games.":
        "This Final Year Project Report presents a study and prototype development project on how level design can enhance object discovery and recognition in investigation-based games.",
    "Finally, Chapter 4 concludes the report by summarizing the main findings of the study, discussing its limitations, and suggesting possible directions for future work. Overall, this report aims to provide insights into how game design can be improved to create more engaging and intuitive player experiences.":
        "Chapter 4 presents the system design and implementation of the Unity-based investigation game prototype. Chapter 5 discusses functional testing and requirement-based evaluation. Chapter 6 concludes the report by summarizing the project outcomes, limitations, and future work. Overall, this report aims to show how strategic clue placement and spatial level design can be applied in a playable investigation game prototype.",
    "Chapter 4: Conclusion":
        "Chapter 4: System Design and Implementation",
}


def main():
    temp = DOCX.with_suffix(".tmp.docx")
    shutil.copy2(DOCX, temp)

    with zipfile.ZipFile(temp, "r") as zin:
        files = {name: zin.read(name) for name in zin.namelist()}

    document_xml = files["word/document.xml"].decode("utf-8", errors="ignore")
    for old, new in REPLACEMENTS.items():
        document_xml = document_xml.replace(html.escape(old, quote=False), html.escape(new, quote=False))

    settings_name = "word/settings.xml"
    if settings_name in files:
        settings_xml = files[settings_name].decode("utf-8", errors="ignore")
        if "<w:updateFields" not in settings_xml:
            settings_xml = settings_xml.replace("</w:settings>", '<w:updateFields w:val="true"/></w:settings>')
            files[settings_name] = settings_xml.encode("utf-8")

    files["word/document.xml"] = document_xml.encode("utf-8")

    with zipfile.ZipFile(DOCX, "w", zipfile.ZIP_DEFLATED) as zout:
        for name, data in files.items():
            zout.writestr(name, data)

    temp.unlink(missing_ok=True)
    print(DOCX)


if __name__ == "__main__":
    main()
