import { createContext, useContext, useState } from "react";

const ReloadContext = createContext({
    reloadKey: 0,
    triggerReload: () => { }
});

export function ReloadProvider({ children }: { children: React.ReactNode }) {
    const [reloadKey, setReloadKey] = useState(0);

    const triggerReload = () => {
        setReloadKey(prev => prev + 1);
    };

    return (
        <ReloadContext.Provider value={{ reloadKey, triggerReload }}>
            {children}
        </ReloadContext.Provider>
    );
}

export function useReload() {
    return useContext(ReloadContext);
}
