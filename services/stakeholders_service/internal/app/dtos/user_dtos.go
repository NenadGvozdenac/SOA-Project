package dtos

type UserResponseDTO struct {
	ID      uint   `json:"id"`
	Name    string `json:"name"`
	Surname string `json:"surname"`
	Email   string `json:"email"`
	Phone   string `json:"phone"`
	Role    string `json:"role"`
}

type BatchUserRequest struct {
	UserIds []string `json:"userIds" binding:"required"`
}

type UserDetailsDTO struct {
	Id             string  `json:"id"`
	Username       string  `json:"username"`
	Name           string  `json:"name"`
	Email          string  `json:"email"`
	ProfilePicture *string `json:"profilePicture"`
}

type UserUpdateDTO struct {
	Name            string `json:"name"`
	Surname         string `json:"surname"`
	Email           string `json:"email"`
	Username        string `json:"username"`
	Biography       string `json:"biography"`
	Moto            string `json:"motto"`
	ProfilePicture  string `json:"profilePicture"` // Base64 encoded image
	OldPassword     string `json:"oldPassword"`
	NewPassword     string `json:"newPassword"`
	ConfirmPassword string `json:"confirmPassword"`
}
