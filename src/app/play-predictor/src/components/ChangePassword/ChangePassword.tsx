'use client'

import { postApiAuthPassword, UserUpdatePassword } from "@/client";
import { useAuth } from "@/lib/authStore";
import { ErrorMessage, SuccessMessage } from "@/lib/message";
import { Button, Field, Fieldset, Separator, Stack } from "@chakra-ui/react";
import { redirect } from "next/navigation";
import { useForm } from "react-hook-form";
import { PasswordInput } from "../ui/password-input";

type ChangePasswordFormData = UserUpdatePassword & {
    repeatPassword: string;
}

export default function ChangePassword() {
    const { setAccessToken, setUser } = useAuth.getState()

    const {
        register,
        handleSubmit,
        watch,
        formState: { errors, isSubmitting },
    } = useForm<ChangePasswordFormData>();

    const newPassword = watch('newPassword'); // Beobachte erstes Passwortfeld

    const onSubmit = handleSubmit(async (data) => {
        const res = await postApiAuthPassword({
            body: {
                currentPassword: data.currentPassword,
                newPassword: data.newPassword
            }
        })

        if (res.response.ok) {
            SuccessMessage("Password Change successful", "You will need to login again.")
            setAccessToken(null)
            setUser(undefined)
            redirect("/auth/login")

        } else if (res.error) {
            ErrorMessage(res.error)
        } else {
            ErrorMessage({ title: "A unknown error occurred" })
        }

    })
    return (
        <Stack padding="4" borderWidth="1px">
            <form onSubmit={onSubmit}>
                <Fieldset.Root size="lg" maxW="lg">
                    <Stack>
                        <Fieldset.Legend>Change Password</Fieldset.Legend>
                        <Fieldset.HelperText>
                            Please fill in the fields.
                        </Fieldset.HelperText>

                        <Fieldset.Content>

                            <Field.Root invalid={!!errors.currentPassword}>
                                <Field.Label>Current Password</Field.Label>
                                <PasswordInput
                                    {...register('currentPassword', { required: 'Current Password is required' })}
                                    placeholder="••••••••"
                                    aria-invalid={errors.currentPassword ? 'true' : 'false'}
                                />
                                <Field.ErrorText>{errors.currentPassword?.message}</Field.ErrorText>
                            </Field.Root>

                            <Separator />

                            <Field.Root invalid={!!errors.newPassword}>
                                <Field.Label>New Password</Field.Label>
                                <PasswordInput
                                    placeholder="••••••••"
                                    {...register('newPassword', {
                                        required: 'New Password is required',
                                        minLength: {
                                            value: 6,
                                            message: 'Password must be at least 6 characters long',
                                        },
                                    })}
                                    aria-invalid={errors.newPassword ? 'true' : 'false'}
                                />
                                <Field.ErrorText>{errors.newPassword?.message}</Field.ErrorText>
                            </Field.Root>


                            <Field.Root invalid={!!errors.repeatPassword}>
                                <Field.Label>Repeat new Password</Field.Label>
                                <PasswordInput
                                    placeholder="••••••••"
                                    {...register('repeatPassword', {
                                        required: 'Please repeat your password',
                                        validate: (value) =>
                                            value === newPassword || 'Passwords do not match',
                                    })}
                                    aria-invalid={errors.repeatPassword ? 'true' : 'false'}
                                />
                                <Field.ErrorText>
                                    {errors.repeatPassword?.message}
                                </Field.ErrorText>
                            </Field.Root>
                        </Fieldset.Content>

                        <Button type="submit" loading={isSubmitting}>
                            Submit
                        </Button>
                    </Stack>
                </Fieldset.Root>
            </form>
        </Stack>
    );
}