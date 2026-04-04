import { getApiOAuthFaceitPlayers, Player, ProblemDetails } from "@/client"
import { create } from "zustand/react";

type ExtServiceState = {
    faceitAccount: Player | null;
    setFaceitAccount: (player: Player) => void;
    getFaceitState: () => Promise<Player | ProblemDetails>,
}

export const useExtServices = create<ExtServiceState>((set) => ({
    faceitAccount: null,
    setFaceitAccount(player) {
        set({ faceitAccount: player })
    },
    async getFaceitState() {
        const res = await getApiOAuthFaceitPlayers();
        if (!res.error) {
            this.setFaceitAccount(res.data)
            return res.data
        } else {
            return res.error
        }
    }
}));