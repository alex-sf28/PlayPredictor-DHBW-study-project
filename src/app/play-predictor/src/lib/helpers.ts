export const sleep = (ms: number) => new Promise<void>(r => setTimeout(r, ms));

export function filterRecordValues(
    data: Record<string, Record<string, any>>,
    allowedKeys: string[]
) {
    const normalizedKeys = allowedKeys.map(k => k.toLowerCase());

    return Object.fromEntries(
        Object.entries(data).map(([outerKey, innerObj]) => [
            outerKey,
            Object.fromEntries(
                Object.entries(innerObj).filter(([key]) =>
                    normalizedKeys.includes(key.toLowerCase())
                )
            )
        ])
    );
}

export function prettyLabeling(raw: string) {
    return raw
        // Großbuchstaben trennen
        .replace(/([A-Z])/g, ' $1')
        // Erstes Zeichen groß machen
        .replace(/^./, s => s.toUpperCase())
        .trim();
}