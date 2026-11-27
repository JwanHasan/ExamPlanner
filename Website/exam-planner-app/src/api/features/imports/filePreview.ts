import type { CellObject, WorkSheet } from "xlsx";
import type { PreviewSheet } from "./importTypes";

export const MAX_PREVIEW_ROWS = 500;

const SYSTEM_PREFIX = "(Skal ikke ændres)";

interface DateParts {
    y: number;
    m: number;
    d: number;
    H: number;
    M: number;
}

interface DateFormats {
    is_date: (format: string | number) => boolean;
    parse_date_code: (value: number, options?: { date1904?: boolean }) => DateParts;
}

const pad = (value: number) => String(value).padStart(2, "0");

export const readFilePreview = async (file: File): Promise<PreviewSheet[]> => {
    const data = new Uint8Array(await file.arrayBuffer());
    if (data[0] !== 0x50 || data[1] !== 0x4b) throw new Error("Not an Excel workbook");

    const XLSX = await import("xlsx");
    const workbook = XLSX.read(data, { type: "array", cellNF: true, cellHTML: false, cellFormula: false });
    const formats = XLSX.SSF as DateFormats;
    const date1904 = Boolean(workbook.Workbook?.WBProps?.date1904);
    const sheetInfo = workbook.Workbook?.Sheets ?? [];

    // ---------- Cells ----------

    const cellText = (cell: CellObject | undefined): string => {
        if (!cell || cell.v === undefined || cell.v === null) return "";

        if (cell.t === "n" && typeof cell.v === "number") {
            if (cell.z !== undefined && formats.is_date(cell.z)) {
                const date = formats.parse_date_code(cell.v, { date1904 });
                const day = `${pad(date.d)}-${pad(date.m)}-${date.y}`;
                return date.H || date.M ? `${day} ${pad(date.H)}:${pad(date.M)}` : day;
            }
            return String(cell.v);
        }
        if (cell.t === "b") return cell.v ? "TRUE" : "FALSE";
        if (cell.t === "e") return cell.w ?? "#ERROR";
        return String(cell.v).trim();
    };

    // ---------- Sheets ----------

    const readSheet = (name: string, sheet: WorkSheet): PreviewSheet => {
        const byRow = new Map<number, Map<number, string>>();
        let firstColumn = Infinity;
        let lastColumn = -1;

        for (const address of Object.keys(sheet)) {
            if (address.startsWith("!")) continue;
            const text = cellText(sheet[address] as CellObject);
            if (!text) continue;

            const { r, c } = XLSX.utils.decode_cell(address);
            if (!byRow.has(r)) byRow.set(r, new Map());
            byRow.get(r)!.set(c, text);
            firstColumn = Math.min(firstColumn, c);
            lastColumn = Math.max(lastColumn, c);
        }

        const rowNumbers = [...byRow.keys()].sort((a, b) => a - b);
        const headerAt = rowNumbers.findIndex(r => byRow.get(r)!.size >= 2);
        if (headerAt < 0) return { name, columns: [], rows: [], totalRows: 0 };

        const columnIndexes = Array.from({ length: lastColumn - firstColumn + 1 }, (_, i) => firstColumn + i);
        const header = byRow.get(rowNumbers[headerAt])!;
        const dataRows = rowNumbers.slice(headerAt + 1);

        return {
            name,
            columns: columnIndexes.map(c => {
                const label = header.get(c) ?? "";
                const system = label.startsWith(SYSTEM_PREFIX);
                return {
                    label: (system ? label.slice(SYSTEM_PREFIX.length).trim() : label) || `Column ${XLSX.utils.encode_col(c)}`,
                    system
                };
            }),
            rows: dataRows.slice(0, MAX_PREVIEW_ROWS).map(r => ({
                excelRow: r + 1,
                cells: columnIndexes.map(c => byRow.get(r)!.get(c) ?? "")
            })),
            totalRows: dataRows.length
        };
    };

    return workbook.SheetNames
        .filter((_, index) => !sheetInfo[index]?.Hidden)
        .map(name => readSheet(name, workbook.Sheets[name]));
};
