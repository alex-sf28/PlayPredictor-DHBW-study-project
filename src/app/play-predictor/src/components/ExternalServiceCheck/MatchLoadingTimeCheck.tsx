import { getApiOAuthFaceitMatchesState } from "@/client";
import { useEffect, useState } from "react";
import AppAlert from "../Alert/AppAlert";

export default function MatchLoadingTimeCheck() {
    const [state, setState] = useState<boolean | undefined>()

    useEffect(() => {
        (async () => {
            const res = await getApiOAuthFaceitMatchesState()
            setState(res.data)
        })()
    }, [])

    if (typeof (state) == "boolean" && !state)
        return <AppAlert
            status={"info"}
            title="Notice"
            description="The first time loading the analytics or prognosis with a new FACEIT account can take some time. Please stay on the page."
        />
}