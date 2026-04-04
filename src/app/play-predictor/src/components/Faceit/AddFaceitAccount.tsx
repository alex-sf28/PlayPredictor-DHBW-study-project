import { getApiOAuthFaceitPlayersSearch, Player, putApiOAuthFaceitPlayersById } from "@/client";
import { ErrorMessage, SuccessMessage } from "@/lib/message";
import { Avatar, Button, Combobox, HStack, Portal, Span, useListCollection } from "@chakra-ui/react"
import { useEffect, useMemo, useState } from "react"
import debounce from "lodash/debounce";
import UserCellSkeleton from "../Skeletons/UserCellSkeleton";

export default function ConnectFaceit({ player }: { player?: Player }) {
    const [inputValue, setInputValue] = useState("")
    const [state, setState] = useState<{ loading: boolean; error: string | null }>({
        loading: false,
        error: null,
    })
    const [loading, setLoading] = useState<boolean>(false)
    const [selected, setSelected] = useState<boolean>(false)


    const { collection, set } = useListCollection<Player>({
        initialItems: [],
        itemToString: (item) => item.nickname ?? "",
        itemToValue: (item) => item.player_id ?? "",
    })

    useEffect(() => {
        if (inputValue.length < 3) {
            set([])
            setState({ loading: false, error: null })
            return
        }
        (async () => {
            setState({ loading: true, error: null })
            const res = await getApiOAuthFaceitPlayersSearch({ query: { nickname: inputValue } })

            if (res.error) {
                setState({ loading: false, error: res.error.title || "Error while fetching Faceit players" })
            } else {
                set(res.data)
                setState({ loading: false, error: null })
            }
        })()

    }, [inputValue, set])

    async function handleSubmit() {
        const selectedPlayer = collection.items?.find((player) => player.nickname === inputValue)

        if (selectedPlayer && selectedPlayer.player_id) {
            setLoading(true)
            const res = await putApiOAuthFaceitPlayersById({ path: { id: selectedPlayer.player_id } })

            if (res.error) {
                ErrorMessage(res.error)
            } else {
                SuccessMessage("Success", "Faceit account connected successfully")
                setLoading(false)
                window.location.reload()
            }
        } else {
            ErrorMessage({ title: "No player selected", detail: "Please select a player from the list" })
        }
    }

    function itemClicked() {
        setSelected(true)
    }

    const onInputChanged = useMemo(
        () => {
            setState(() => ({ loading: true, error: null }))
            return debounce((nextValue: string) => {
                setInputValue(nextValue)
            }, 500)
        }, []
    );

    return (
        <>
            <Combobox.Root
                width="sm"
                collection={collection}
                onInputValueChange={(e) => onInputChanged(e.inputValue)}
                positioning={{ sameWidth: true, placement: "bottom-start" }}
                defaultInputValue={player?.nickname ?? ""}
                defaultValue={[player?.player_id ?? ""]}
            >
                <Combobox.Label>Select your Faceit Account</Combobox.Label>

                <Combobox.Control>
                    <Combobox.Input placeholder="Type in username" />
                    <Combobox.IndicatorGroup>
                        <Combobox.ClearTrigger onClick={() => setSelected(false)} />

                    </Combobox.IndicatorGroup>
                </Combobox.Control>

                <Portal>
                    <Combobox.Positioner>
                        <Combobox.Content minW="sm">
                            {state.loading ? (
                                <UserCellSkeleton />
                            ) : state.error ? (
                                <Span p="2" color="fg.error">
                                    {state.error}
                                </Span>
                            ) : (
                                collection.items?.map((player) => (
                                    <Combobox.Item key={player.player_id} item={player} onClick={itemClicked}>
                                        <HStack justify="space-between" textStyle="sm">
                                            <Avatar.Root>
                                                <Avatar.Image src={player.avatar ? player.avatar : undefined} />
                                                <Avatar.Fallback />
                                            </Avatar.Root>
                                            <Span fontWeight="medium" truncate>
                                                {player.nickname}
                                            </Span>
                                        </HStack>
                                        <Combobox.ItemIndicator />
                                    </Combobox.Item>
                                ))
                            )}
                        </Combobox.Content>
                    </Combobox.Positioner>
                </Portal>
            </Combobox.Root>
            <Button marginTop={4} onClick={handleSubmit} loading={loading} disabled={!selected}>Submit</Button>
        </>
    )
}