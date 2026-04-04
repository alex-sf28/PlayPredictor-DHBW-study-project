import { Button, VStack } from "@chakra-ui/react";
import { FileUpload } from "@chakra-ui/react";
import { HiUpload } from "react-icons/hi";
import { useState } from "react";
import { postApiOAuthGoogleCalendarCalendars } from "@/client";
import { ErrorMessage, SuccessMessage } from "@/lib/message";

export default function AddICalendarFiles() {
    const [files, setFiles] = useState<File[]>([]);
    const [loading, setLoading] = useState<boolean>(false)

    const postApiCalendars = async (files: File[]) => {
        const formData = new FormData();

        files.forEach((file) => {
            formData.append("files", file);
        });

        setLoading(true)
        const res = await postApiOAuthGoogleCalendarCalendars({
            body: {
                files: files
            }
        })
        if (res.error) {
            ErrorMessage(res.error)
        } else {
            SuccessMessage("Success", "Calendars uploaded successfully")
        }
        setLoading(false)
        window.location.reload()
    };

    return (
        <FileUpload.Root
            maxFiles={20}
            accept={["text/calendar"]}
            onFileAccept={(details) => {
                setFiles(details.files);
            }}
        >
            <FileUpload.HiddenInput />

            <VStack align="start">
                <FileUpload.Trigger asChild>
                    <Button variant="outline" size="sm">
                        <HiUpload /> Upload calendar files (.ics)
                    </Button>
                </FileUpload.Trigger>

                <FileUpload.List showSize clearable />

                <Button
                    onClick={() => postApiCalendars(files)}
                    disabled={files.length === 0}
                    loading={loading}
                >
                    Upload Files
                </Button>
            </VStack>
        </FileUpload.Root>
    );
}