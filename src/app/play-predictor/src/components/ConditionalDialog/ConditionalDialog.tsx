import { useState } from "react";
import { Button, CloseButton, Dialog, Portal } from "@chakra-ui/react";

export type ConditionalDialogProps = {
    onTrue: () => Promise<void>;
    children: React.ReactNode;
    title: string;
    detail: string;
    trueButtonText: React.ReactNode;
    highlight?: boolean;
    loading: boolean;
};

export function ConditionalDialog({
    onTrue,
    children,
    title,
    detail,
    trueButtonText,
    highlight,
    loading,
}: ConditionalDialogProps) {
    const [open, setOpen] = useState(false);

    // Wrapper-Funktion, die nach Abschluss von onTrue den Dialog schließt
    const handleTrue = async () => {
        await onTrue();
        setOpen(false);
    };

    return (
        <Dialog.Root open={open} onOpenChange={(e) => setOpen(e.open)}>
            <Dialog.Trigger asChild>
                {children}
            </Dialog.Trigger>

            <Portal>
                <Dialog.Backdrop />
                <Dialog.Positioner>
                    <Dialog.Content>
                        <Dialog.Header>
                            <Dialog.Title>{title}</Dialog.Title>
                        </Dialog.Header>

                        <Dialog.Body>
                            <p>{detail}</p>
                        </Dialog.Body>

                        <Dialog.Footer>
                            <Dialog.ActionTrigger asChild>
                                <Button variant="outline">Cancel</Button>
                            </Dialog.ActionTrigger>

                            <Button
                                bg={highlight ? "danger.500" : "current"}
                                color={highlight ? "white" : "current"}
                                onClick={handleTrue}
                                loading={loading}
                            >
                                {trueButtonText}
                            </Button>
                        </Dialog.Footer>

                        <Dialog.CloseTrigger asChild>
                            <CloseButton size="sm" />
                        </Dialog.CloseTrigger>
                    </Dialog.Content>
                </Dialog.Positioner>
            </Portal>
        </Dialog.Root>
    );
}
