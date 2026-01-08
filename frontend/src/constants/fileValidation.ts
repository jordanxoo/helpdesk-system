// File upload validation constants
export const FILE_UPLOAD_LIMITS = {
    MAX_SIZE: 10 * 1024 * 1024, // 10 MB
    ALLOWED_MIME_TYPES: ['image/jpeg', 'image/png', 'image/gif', 'image/webp', 'application/pdf'],
    ALLOWED_EXTENSIONS: ['.jpg', '.jpeg', '.png', '.gif', '.webp', '.pdf'],
    ACCEPT_ATTRIBUTE: 'image/jpeg,image/png,image/gif,image/webp,application/pdf',
} as const;

export const validateFile = (file: File): { isValid: boolean; error?: string } => {
    // Check file size
    if (file.size > FILE_UPLOAD_LIMITS.MAX_SIZE) {
        return {
            isValid: false,
            error: `Plik "${file.name}" jest za duży. Maksymalny rozmiar to 10 MB.`
        };
    }

    // Extract and validate file extension
    const lastDotIndex = file.name.lastIndexOf('.');
    if (lastDotIndex === -1) {
        return {
            isValid: false,
            error: `Plik "${file.name}" nie ma rozszerzenia. Dozwolone: JPG, PNG, GIF, WEBP, PDF.`
        };
    }

    const fileExtension = file.name.substring(lastDotIndex).toLowerCase();
    
    // Validate MIME type and extension
    if (!FILE_UPLOAD_LIMITS.ALLOWED_MIME_TYPES.includes(file.type as any) && 
        !FILE_UPLOAD_LIMITS.ALLOWED_EXTENSIONS.includes(fileExtension as any)) {
        return {
            isValid: false,
            error: `Plik "${file.name}" ma nieobsługiwany format. Dozwolone: JPG, PNG, GIF, WEBP, PDF.`
        };
    }

    return { isValid: true };
};
